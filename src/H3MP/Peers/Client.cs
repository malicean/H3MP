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
	public class Client : Peer<InputSnapshotMessage, ClientConfig>
	{
		public delegate void DeltaSnapshotReceivedHandler(Option<BufferTicks> bufferTicks, uint sentTick, DeltaWorldSnapshotMessage delta);
		public delegate void SnapshotReceivedHandler(WorldSnapshotMessage snapshot);

		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<Tickstamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<ResponseTickstamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		public readonly int MaxPlayers;

		private Option<InputSnapshotMessage> _lastSent;
		private Option<WorldSnapshotMessage> _lastReceived;

		public readonly int SnapshotCount;
		public readonly List<KeyValuePair<LocalTickstamp, WorldSnapshotMessage>> Snapshots;
		public readonly IFitter<WorldSnapshotMessage> SnapshotsFitter;
		public readonly DataSetFitter<uint, WorldSnapshotMessage> TickSnapshotsDataFitter;
		public readonly DataSetFitter<double, WorldSnapshotMessage> TimeSnapshotsDataFitter;

		public event DeltaSnapshotReceivedHandler DeltaSnapshotReceived;
		public event SnapshotReceivedHandler SnapshotUpdated;
		public event Action<DisconnectInfo> Disconnected;

		public Client(Log log, ClientConfig config, TickFrameClock clock, int maxPlayers) : base(log, config, clock)
		{
			_inputDiff = new InputSnapshotMessageDifferentiator();
			_worldDiff = new WorldSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new ResponseTickstampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(maxPlayers));

			MaxPlayers = maxPlayers;

			_lastSent = Option.None<InputSnapshotMessage>();

			SnapshotCount = (int) Math.Ceiling(5 / clock.DeltaTime);
			Snapshots = new List<KeyValuePair<LocalTickstamp, WorldSnapshotMessage>>(SnapshotCount);
			SnapshotsFitter = new WorldSnapshotMessageFitter();
			TickSnapshotsDataFitter = new DataSetFitter<uint, WorldSnapshotMessage>(Comparer<uint>.Default, InverseFitters.UInt, SnapshotsFitter);
			TimeSnapshotsDataFitter = new DataSetFitter<double, WorldSnapshotMessage>(Comparer<double>.Default, InverseFitters.Double, SnapshotsFitter);
			_lastReceived = Option.None<WorldSnapshotMessage>();

			Listener.PeerDisconnectedEvent += InternalDisconnected;

			Simulate += RunSimulation;
		}

		private void InternalDisconnected(NetPeer peer, DisconnectInfo info)
		{
			Disconnected?.Invoke(info);
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			ResponseTickstamped<DeltaWorldSnapshotMessage> delta;
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

			var baseline = Snapshots.LastOrNone().Map(x => x.Value);
			var snapshot = _worldDiff.ConsumeDelta(delta.Content, baseline);

			// TODO: use sent tick to determine time and tick
			Snapshots.Add(new KeyValuePair<LocalTickstamp, WorldSnapshotMessage>(new LocalTickstamp(Clock.Time, Clock.Tick), snapshot));
			_lastReceived = Option.Some(snapshot);

			DeltaSnapshotReceived?.Invoke(delta.Buffer, delta.SentTick, delta.Content);
		}

		private void RunSimulation()
		{
			if (_lastReceived.MatchSome(out var snapshot))
			{
				SnapshotUpdated?.Invoke(snapshot);
			}

			_lastReceived = Option.None<WorldSnapshotMessage>();

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

			_inputSerializer.Serialize(ref writer, new Tickstamped<DeltaInputSnapshotMessage>
			{
				Content = delta
			});

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
