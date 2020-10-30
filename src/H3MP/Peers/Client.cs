using System;
using System.Collections.Generic;
using System.Net;
using BepInEx.Logging;
using FistVR;
using H3MP.Differentiation;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Serialization;
using H3MP.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Peers
{
	public class Client : Peer
	{
		private readonly IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage> _inputDiff;
		private readonly IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage> _worldDiff;

		private readonly ISerializer<DeltaInputSnapshotMessage> _inputSerializer;
		private readonly ISerializer<DeltaWorldSnapshotMessage> _worldSerializer;

		public readonly List<KeyValuePair<uint, WorldSnapshotMessage>> WorldSnapshots;
		public readonly DataSetFitter<uint, WorldSnapshotMessage> WorldSnapshotsFitter;

		private Option<InputSnapshotMessage> _inputSnapshotOld;
		public InputSnapshotMessage InputSnapshot;

		public Client(ManualLogSource log, double tickStep) : base(log, tickStep)
		{
			_inputDiff = new InputSnapshotMessageDifferentiator();
			_worldDiff = new WorldSnapshotMessageDifferentiator();

			_inputSerializer = new DeltaInputSnapshotSerializer();
			_inputSerializer = new DeltaWorldSnapshotSerializer();

			WorldSnapshots = new List<KeyValuePair<uint, WorldSnapshotMessage>>((int) (5 / tickStep));
			WorldSnapshotsFitter = new DataSetFitter<uint, WorldSnapshotMessage>(Comparer<uint>.Default, InverseFitters.UInt, new WorldSnapshotMessageFitter());

			_inputSnapshotOld = Option.None<InputSnapshotMessage>();
			InputSnapshot = new InputSnapshotMessage();

			Listener.NetworkReceiveEvent += NetworkReceive;
		}

		private void NetworkReceive(NetPeer peer, NetPacketReader netReader, DeliveryMethod deliveryMethod)
		{
			var reader = new BitPackReader(netReader);

			var tickstamped = _worldSerializer.Deserialize(ref reader);
			var baseline = WorldSnapshots.Count > 0
				? Option.Some(WorldSnapshots[WorldSnapshots.Count - 1].Value)
				: Option.None<WorldSnapshotMessage>();
			var snapshot = _worldDiff.ConsumeDelta(tickstamped.Content, baseline);

			WorldSnapshots.Add(new KeyValuePair<uint, WorldSnapshotMessage>(tickstamped.Tick, snapshot));
		}

		public void Connect(IPEndPoint endPoint, NetDataWriter data)
		{
			Net.Start();
			Net.Connect(endPoint, data);
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

			InputSnapshot.Body = new BodyMessage
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

        protected override bool NetUpdate()
        {
            if (!base.NetUpdate())
			{
				return false;
			}

			InputSnapshot = default;

			SetBody();

			var writer = new BitPackWriter(new NetDataWriter());
			_inputSerializer.Serialize(ref writer, InputSnapshot, _inputSnapshotOld);

			_inputSnapshotOld = Option.Some(InputSnapshot);

			return true;
        }
    }
}
