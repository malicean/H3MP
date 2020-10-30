using System;
using System.Collections.Generic;
using System.Net;
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
		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<ConnectionRequestMessage> _requestSerializer;
		private readonly ISerializer<TickstampedMessage<DeltaInputSnapshotMessage>> _inputSerializer;
		private readonly ISerializer<TickstampedResponseMessage<DeltaWorldSnapshotMessage>> _worldSerializer;

		private Option<InputSnapshotMessage> _oldSnapshot;

		public readonly List<KeyValuePair<uint, WorldSnapshotMessage>> WorldSnapshots;
		public readonly DataSetFitter<uint, WorldSnapshotMessage> WorldSnapshotsFitter;

		public event Action<DisconnectInfo> Disconnected;

		public Client(Log log, ClientConfig config, double tickStep, int playerCount) : base(log, config, tickStep)
		{
			_inputDiff = new InputSnapshotMessageDifferentiator();
			_worldDiff = new WorldSnapshotMessageDifferentiator();

			_requestSerializer = new ConnectionRequestSerializer();
			_inputSerializer = new TickstampedMessageSerializer<DeltaInputSnapshotMessage>(new DeltaInputSnapshotSerializer());
			_worldSerializer = new TickstampedResponseMessageSerializer<DeltaWorldSnapshotMessage>(new DeltaWorldSnapshotSerializer(playerCount));

			_oldSnapshot = Option.None<InputSnapshotMessage>();

			WorldSnapshots = new List<KeyValuePair<uint, WorldSnapshotMessage>>((int) (5 / tickStep));
			WorldSnapshotsFitter = new DataSetFitter<uint, WorldSnapshotMessage>(Comparer<uint>.Default, InverseFitters.UInt, new WorldSnapshotMessageFitter());

			Listener.PeerDisconnectedEvent += InternalDisconnected;
		}

		private void InternalDisconnected(NetPeer peer, DisconnectInfo info)
		{
			Disconnected?.Invoke(info);
		}

		private void SetBody()
		{
			var body = GM.CurrentPlayerBody;

			TransformMessage GetLocal(Transform transform)
			{
				return new TransformMessage
				{
					Position = transform.localPosition,
					Rotation = transform.localRotation
				};
			}

			LocalSnapshot.Body = new BodyMessage
			{
				Root = new TransformMessage
				{
					Position = body.transform.position,
					Rotation = body.transform.rotation
				},
				Head = GetLocal(body.Head),
				HandLeft = GetLocal(body.LeftHand),
				HandRight = GetLocal(body.RightHand)
			};
		}

		protected override void ReceiveDelta(NetPeer peer, ref BitPackReader reader)
		{
			var tickstamped = _worldSerializer.Deserialize(ref reader);
			var baseline = WorldSnapshots.LastOrNone().Map(x => x.Value);
			var snapshot = _worldDiff.ConsumeDelta(tickstamped.Content, baseline);

			WorldSnapshots.Add(new KeyValuePair<uint, WorldSnapshotMessage>(tickstamped.Tick, snapshot));
		}

		protected override void SendSnapshot(InputSnapshotMessage snapshot)
		{
			if (!_inputDiff.CreateDelta(snapshot, _oldSnapshot).MatchSome(out var delta))
			{
				return;
			}

			var data = new NetDataWriter();
			var writer = new BitPackWriter(data);

			_inputSerializer.Serialize(ref writer, new TickstampedMessage<DeltaInputSnapshotMessage>
			{
				Tick = Tick,
				Content = delta
			});

			writer.Dispose();
			Net.SendToAll(data, DeliveryMethod.ReliableOrdered);
		}

        protected override void OnNetUpdate()
        {
			base.OnNetUpdate();

			SetBody();
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
