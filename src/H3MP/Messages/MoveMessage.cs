using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
    public struct MoveMessage : INetSerializable, IDeltable<MoveMessage, MoveMessage>, ILinearFittable<MoveMessage>, IEquatable<MoveMessage>
	{
		public Option<TransformMessage> Head { get; private set; }

		public Option<TransformMessage> HandLeft { get; private set; }

		public Option<TransformMessage> HandRight { get; private set; }

		public MoveMessage InitialDelta => this;

		public MoveMessage(Option<TransformMessage> head, Option<TransformMessage> handLeft, Option<TransformMessage> handRight)
		{
			Head = head;
			HandLeft = handLeft;
			HandRight = handRight;
		}

		public MoveMessage(Transform head, Transform handLeft, Transform handRight) : this(
			Option.Some(new TransformMessage(head)),
			Option.Some(new TransformMessage(handLeft)),
			Option.Some(new TransformMessage(handRight))
		) { }

		public void Deserialize(NetDataReader reader)
		{
			Head = reader.Get<TransformMessage>();
			HandLeft = reader.Get<TransformMessage>();
			HandRight = reader.Get<TransformMessage>();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Head);
			writer.Put(HandLeft);
			writer.Put(HandRight);
		}

		public Option<MoveMessage> CreateDelta(MoveMessage head)
        {
            return Option.Some(new MoveMessage(
				Head.CreateDelta(head.Head),
				HandLeft.CreateDelta(head.HandLeft),
				HandRight.CreateDelta(head.HandRight)
			));
        }

		public MoveMessage ConsumeDelta(MoveMessage head)
        {
            return new MoveMessage(
				Head.ConsumeDelta(head.Head),
				HandLeft.ConsumeDelta(head.HandLeft),
				HandRight.ConsumeDelta(head.HandRight)
			);
        }

		public MoveMessage Fit(MoveMessage b, double t)
		{
			return new MoveMessage(
				Head.Fit(b.Head, t),
				HandLeft.Fit(b.HandLeft, t),
				HandRight.Fit(b.HandRight, t)
			);
		}

        public bool Equals(MoveMessage other)
        {
            return Head.Equals(other.Head) && HandLeft.Equals(other.HandLeft) && HandRight.Equals(other.HandRight);
        }
    }
}
