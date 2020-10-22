using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
    public struct MoveMessage : INetSerializable, IDeltable<MoveMessage>, ILinearFittable<MoveMessage>, IEquatable<MoveMessage>
	{
		public TransformMessage Head { get; private set; }

		public TransformMessage HandLeft { get; private set; }

		public TransformMessage HandRight { get; private set; }

		public MoveMessage(TransformMessage head, TransformMessage handLeft, TransformMessage handRight)
		{
			Head = head;
			HandLeft = handLeft;
			HandRight = handRight;
		}

		public MoveMessage(Transform head, Transform handLeft, Transform handRight) : this(new TransformMessage(head), new TransformMessage(handLeft), new TransformMessage(handRight))
		{
		}

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

		public MoveMessage CreateDelta(MoveMessage head)
        {
            return new MoveMessage(
				Head.CreateDelta(head.Head),
				HandLeft.CreateDelta(head.HandLeft),
				HandRight.CreateDelta(head.HandRight)
			);
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
