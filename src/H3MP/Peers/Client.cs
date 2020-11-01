using System;
using System.Collections.Generic;
using System.Net;
using Discord;
using FistVR;
using H3MP.Configs;
using H3MP.Differentiation;
using H3MP.Extensions;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Peers
{
	public class Client : Peer<InputSnapshotMessage, ClientConfig>
	{
		public delegate void DeltaSnapshotReceivedHandler(Option<BufferTicks> bufferTicks, uint sentTick, DeltaWorldSnapshotMessage delta);
		public delegate void SnapshotReceivedHandler(Option<BufferTicks> bufferTicks, uint sentTick, WorldSnapshotMessage snapshot);

		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<Tickstamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<ResponseTickstamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		private Option<InputSnapshotMessage> _oldSnapshot;

		private Option<long> _serverTickOffset;
		public Option<uint> OffsetTick => _serverTickOffset.MatchSome(out var offset) ? Option.Some((uint) (Tick + offset)) : Option.None<uint>();

		public readonly List<KeyValuePair<uint, WorldSnapshotMessage>> WorldSnapshots;
		public readonly DataSetFitter<uint, WorldSnapshotMessage> WorldSnapshotsFitter;

		public event DeltaSnapshotReceivedHandler DeltaSnapshotReceived;
		public event SnapshotReceivedHandler SnapshotUpdated;
		public event Action<DisconnectInfo> Disconnected;

		public Client(Log log, ClientConfig config, double tickStep, int playerCount) : base(log, config, tickStep)
		{
			_inputDiff = new InputSnapshotMessageDifferentiator();
			_worldDiff = new WorldSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new ResponseTickstampedSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(playerCount));

			_oldSnapshot = Option.None<InputSnapshotMessage>();

			WorldSnapshots = new List<KeyValuePair<uint, WorldSnapshotMessage>>((int) (5 / tickStep));
			WorldSnapshotsFitter = new DataSetFitter<uint, WorldSnapshotMessage>(Comparer<uint>.Default, InverseFitters.UInt, new WorldSnapshotMessageFitter());

			Listener.PeerDisconnectedEvent += InternalDisconnected;

			DeltaSnapshotReceived += UpdateTick;
		}

		private void UpdateTick(Option<BufferTicks> bufferTicks, uint sentTick, DeltaWorldSnapshotMessage delta)
		{
			if (bufferTicks.MatchSome(out var bufferTicksValue))
			{
				// TODO: make it work
				long sentOffset = bufferTicksValue.Queued - bufferTicksValue.Received;

				if (_serverTickOffset.MatchSome(out var offset))
				{
					// Adjust via binary exponential decay

					var adjustmentRaw = sentOffset - offset;
					var adjustmentSoftened = adjustmentRaw > 1 ? adjustmentRaw / 2 : adjustmentRaw;

					_serverTickOffset = Option.Some(offset + adjustmentSoftened);
				}
				else
				{
					// Tick should already be set at this point, but for the edge case.

					_serverTickOffset = Option.Some(sentOffset);
				}
			}
			else
			{
				// RTT: unknown

				_serverTickOffset = Option.Some((long) (sentTick - Tick));
			}
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

			var baseline = WorldSnapshots.LastOrNone().Map(x => x.Value);
			var snapshot = _worldDiff.ConsumeDelta(delta.Content, baseline);

			WorldSnapshots.Add(new KeyValuePair<uint, WorldSnapshotMessage>(delta.SentTick, snapshot));

			DeltaSnapshotReceived?.Invoke(delta.Buffer, delta.SentTick, delta.Content);
			SnapshotUpdated?.Invoke(delta.Buffer, delta.SentTick, snapshot);
		}

		protected override void SendSnapshot(InputSnapshotMessage snapshot)
		{
			// Nothing is sent before world info
			if (!OffsetTick.MatchSome(out var tick))
			{
				return;
			}

			if (!_inputDiff.CreateDelta(snapshot, _oldSnapshot).MatchSome(out var delta))
			{
				return;
			}

			var data = new NetDataWriter();
			var writer = new BitPackWriter(data);

			_inputSerializer.Serialize(ref writer, new Tickstamped<DeltaInputSnapshotMessage>
			{
				Tick = tick,
				Content = delta
			});

			writer.Dispose();
			Net.SendToAll(data, DeliveryMethod.ReliableOrdered);
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
