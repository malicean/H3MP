using System;
using System.Collections.Generic;
using System.Net;
using H3MP.Configs;
using H3MP.Differentiation;
using H3MP.Extensions;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Timing;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Peers
{
	public class Client : Peer<Client, InputSnapshotMessage, ClientConfig>
	{
		public delegate void DeltaSnapshotReceivedHandler(Client client, TickTimeStamped<DeltaWorldSnapshotMessage> delta);
		public delegate void SnapshotReceivedHandler(Client client, TickTimeStamped<WorldSnapshotMessage> snapshot);

		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<TickStamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<TickStamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		public readonly int MaxPlayers;

		private Option<InputSnapshotMessage> _lastSent;
		private Option<TickTimeStamped<WorldSnapshotMessage>> _lastReceived;

		public readonly int SnapshotCount;
		public readonly List<TickTimeStamped<WorldSnapshotMessage>> Snapshots;
		public readonly IFitter<WorldSnapshotMessage> SnapshotsFitter;
		public readonly DataSetFitter<TickStamped<WorldSnapshotMessage>, long, WorldSnapshotMessage> TickSnapshotsDataFitter;
		public readonly DataSetFitter<TimeStamped<WorldSnapshotMessage>, double, WorldSnapshotMessage> TimeSnapshotsDataFitter;

		protected override Client Self => this;

		public event DeltaSnapshotReceivedHandler DeltaSnapshotReceived;
		public event SnapshotReceivedHandler SnapshotUpdated;
		public event Action<DisconnectInfo> Disconnected;

		public Client(Log log, ClientConfig config, TickFrameClock clock, int maxPlayers) : base(log, config, clock)
		{
			_inputDiff = new InputSnapshotMessageDifferentiator();
			_worldDiff = new WorldSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickStampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new TickStampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			MaxPlayers = maxPlayers;

			_lastSent = Option.None<InputSnapshotMessage>();
			_lastReceived = Option.None<TickTimeStamped<WorldSnapshotMessage>>();

			SnapshotCount = (int) Math.Ceiling(5 / clock.DeltaTime);
			Snapshots = new List<TickTimeStamped<WorldSnapshotMessage>>(SnapshotCount);
			SnapshotsFitter = new WorldSnapshotMessageFitter();
			TickSnapshotsDataFitter = new DataSetFitter<TickStamped<WorldSnapshotMessage>, long, WorldSnapshotMessage>(Comparer<long>.Default, InverseFitters.Long, SnapshotsFitter);
			TimeSnapshotsDataFitter = new DataSetFitter<TimeStamped<WorldSnapshotMessage>, double, WorldSnapshotMessage>(Comparer<double>.Default, InverseFitters.Double, SnapshotsFitter);

			Listener.PeerDisconnectedEvent += InternalDisconnected;

			Simulate += RunSimulation;
		}

		private void InternalDisconnected(NetPeer peer, DisconnectInfo info)
		{
			Disconnected?.Invoke(info);
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			TickStamped<DeltaWorldSnapshotMessage> delta;
			try
			{
				delta = _worldSerializer.Deserialize(ref reader);
			}
			catch (Exception e)
			{
				Log.Common.LogInfo("Received malformed data from server.");
				Log.Common.LogDebug("Malformation error: " + e);

				Net.DisconnectAll();
				return;
			}

			var baseline = Snapshots.LastOrNone().Map(x => x.Content);
			var snapshot = _worldDiff.ConsumeDelta(delta.Content, baseline);

			// TODO: use sent tick to determine time
			var stamp = new TickTimeStamp(delta.Stamp, Clock.Time);
			var deltaStamped = new TickTimeStamped<DeltaWorldSnapshotMessage>(stamp, delta.Content);
			var snapshotStamped = new TickTimeStamped<WorldSnapshotMessage>(stamp, snapshot);

			Snapshots.Add(snapshotStamped);
			_lastReceived = Option.Some(snapshotStamped);

			DeltaSnapshotReceived?.Invoke(this, deltaStamped);
		}

		private void RunSimulation(Client _)
		{
			if (_lastReceived.MatchSome(out var snapshot))
			{
				SnapshotUpdated?.Invoke(this, snapshot);
			}

			_lastReceived = Option.None<TickTimeStamped<WorldSnapshotMessage>>();

			while (Snapshots.Count > SnapshotCount)
			{
				Snapshots.RemoveAt(0);
			}
		}

		protected override void SendSnapshot(InputSnapshotMessage snapshot)
		{
			if (!_inputDiff.CreateDelta(snapshot, _lastSent).MatchSome(out var delta))
			{
				return;
			}

			var data = new NetDataWriter();
			var writer = new BitPackWriter(data);

			var stamped = new TickStamped<DeltaInputSnapshotMessage>(Clock.Tick, delta);
			_inputSerializer.Serialize(ref writer, stamped);

			writer.Dispose();
			Net.SendToAll(data, DeliveryMethod.ReliableOrdered);

			_lastSent = Option.Some(snapshot.Copy());
		}

		public void Connect(IPEndPoint endPoint, ConnectionRequestMessage request)
		{
			var data = new NetDataWriter();
			var writer = new BitPackWriter(data);
			_requestSerializer.Serialize(ref writer, request);
			writer.Dispose();

			Net.Start();
			Net.Connect(endPoint, data);
		}
    }
}
