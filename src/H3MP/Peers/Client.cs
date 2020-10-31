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
		public delegate void DeltaSnapshotReceivedHandler(Option<uint> inputTick, uint serverTick, DeltaWorldSnapshotMessage delta);
		public delegate void SnapshotReceivedHandler(Option<uint> inputTick, uint serverTick, WorldSnapshotMessage delta);

		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<Tickstamped<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<ResponseTickstamped<DeltaWorldSnapshotMessage>> _worldSerializer;

		private Option<InputSnapshotMessage> _oldSnapshot;

		private Option<uint> _serverTickOffset;
		public Option<uint> OffsetTick => _serverTickOffset.MatchSome(out var offset) ? Option.Some(Tick + offset) : Option.None<uint>();

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

			SnapshotUpdated += UpdateTick;
		}

		private void UpdateTick(Option<uint> inputTick, uint serverTick, WorldSnapshotMessage delta)
		{
			if (inputTick.MatchSome(out var inputTickValue))
			{
				// 1/2 RTT ~= serverTick - inputTick
				// 1 RTT ~= 2 * 1/2 RTT
				// tick ~= serverTick + ~1 RTT

				uint rttHalf = serverTick - inputTickValue;
				var rtt = 2 * rttHalf;
				var tick = serverTick + rtt;

				if (OffsetTick.MatchSome(out var nowTick))
				{
					// Adjust via binary exponential decay

					var offset = tick - nowTick;
					var adjustment = offset > 1 ? offset / 2 : offset;

					OffsetTick = Option.Some(nowTick + adjustment);
				}
				else
				{
					// Tick should already be set at this point, but for the edge case.

					OffsetTick = Option.Some(tick);
				}
			}
			else
			{
				// RTT: unknown
				// With no information, this is theoretically 1/2 RTT behind server.

				OffsetTick = Option.Some(serverTick);
			}
		}

		private void InternalDisconnected(NetPeer peer, DisconnectInfo info)
		{
			Disconnected?.Invoke(info);
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			var tickstamped = _worldSerializer.Deserialize(ref reader);
			var baseline = WorldSnapshots.LastOrNone().Map(x => x.Value);
			var snapshot = _worldDiff.ConsumeDelta(tickstamped.Content, baseline);

			WorldSnapshots.Add(new KeyValuePair<uint, WorldSnapshotMessage>(tickstamped.QueuedTick, snapshot));

			DeltaSnapshotReceived?.Invoke(tickstamped.ReceivedTick, tickstamped.QueuedTick, tickstamped.Content);
			SnapshotUpdated?.Invoke(tickstamped.ReceivedTick, tickstamped.QueuedTick, snapshot);
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

		public void Connect(IPEndPoint endPoint, Key32 key, bool isAdmin)
		{
			var data = new NetDataWriter();
			var writer = new BitPackWriter(data);
			_requestSerializer.Serialize(ref writer, new ConnectionRequestMessage
			{
				IsAdmin = isAdmin,
				Key = key
			});
			writer.Dispose();

			Net.Start();
			Net.Connect(endPoint, data);
		}

		public void Disconnect()
		{
			Net.Stop();
		}
    }
}
