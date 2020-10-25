using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
    public struct MoveMessage : IPackedSerializable, IDeltable<MoveMessage, MoveMessage>, IEquatable<MoveMessage>, ILinearFittable<MoveMessage>
	{
		public Option<TransformMessage> Head;
		public Option<TransformMessage> HandLeft;
		public Option<TransformMessage> HandRight;

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

		public void Deserialize(BitPackReader reader)
		{
			Head = reader.Get<TransformMessage>();
			HandLeft = reader.Get<TransformMessage>();
			HandRight = reader.Get<TransformMessage>();
		}

		public void Serialize(BitPackWriter writer)
		{
			writer.Put(Head);
			writer.Put(HandLeft);
			writer.Put(HandRight);
		}

		public Option<MoveMessage> CreateDelta(MoveMessage head)
        {
			var deltas = new DeltaComparer<MoveMessage>(this, head);
			var delta = new MoveMessage
			{
				Head = deltas.Create(x => x.Head),
				HandLeft = deltas.Create(x => x.HandLeft),
				HandRight = deltas.Create(x => x.HandRight)
			};

			return delta.Equals(default) // no delta
				? Option.None<MoveMessage>()
				: Option.Some(delta);
        }

		public MoveMessage ConsumeDelta(MoveMessage head)
        {
			var deltas = new DeltaComparer<MoveMessage>(this, head);
            return new MoveMessage(
				deltas.Consume(x => x.Head),
				deltas.Consume(x => x.HandLeft),
				deltas.Consume(x => x.HandRight)
			);
        }

		public bool Equals(MoveMessage other)
        {
            return Head.Equals(other.Head)
				&& HandLeft.Equals(other.HandLeft)
				&& HandRight.Equals(other.HandRight);
        }

		public MoveMessage Fit(MoveMessage b, double t)
		{
			// local func variable captures use no allocations (lambdas do)
			void Fit(TransformMessage thisTransform, TransformMessage bTransform)
			{
				thisTransform.Fit(bTransform, t);
			}

			return new MoveMessage(
				Head.Map(Fit),
				HandLeft.Map(Fit),
				HandRight.Map(Fit)
			);
		}
    }
}
