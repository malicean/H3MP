using System;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct BodyMessage : IEquatable<BodyMessage>
	{
		public TransformMessage Head;
		public TransformMessage HandLeft;
		public TransformMessage HandRight;

		public bool Equals(BodyMessage other)
        {
            return Head.Equals(other.Head)
				&& HandLeft.Equals(other.HandLeft)
				&& HandRight.Equals(other.HandRight);
        }
    }

	public class MoveMessageFitter : IFitter<BodyMessage>
	{
		private readonly TransformMessageFitter _transform;

		public MoveMessageFitter(TransformMessageFitter transform)
		{
			_transform = transform;
		}

		public BodyMessage Fit(BodyMessage a, BodyMessage b, float t)
		{
			var fitter = new SuperFitter<BodyMessage>(a, b, t);

			return new BodyMessage
			{
				Head = fitter.Fit(x => x.Head, _transform),
				HandLeft = fitter.Fit(x => x.HandLeft, _transform),
				HandRight = fitter.Fit(x => x.HandRight, _transform)
			};
		}
	}

	public class MoveMessageDeltaSerializer : IDeltaSerializer<BodyMessage>
	{
		private IDeltaSerializer<TransformMessage> _head;
		private IDeltaSerializer<TransformMessage> _hand;

		public MoveMessageDeltaSerializer()
		{
			var rot = Differentiators.Quaternion.ToSerializer(new SmallestThreeQuaternionSerializer(PackedSerializers.UFloat(1)));

			var headPos = Differentiators.Vector3.ToSerializer(new Vector3Serializer(PackedSerializers.Float(50))); // In case of sticky jumping
			_head = new TransformMessageDeltaSerializer(headPos, rot);

			var handPos = Differentiators.Vector3.ToSerializer(new Vector3Serializer(PackedSerializers.Float(2))); // Accounts for bald eagles
			_hand = new TransformMessageDeltaSerializer(handPos, rot);
		}

		public BodyMessage Deserialize(ref BitPackReader reader, Option<BodyMessage> baseline)
		{
			return new BodyMessage
			{
				Head = _head.Deserialize(ref reader, baseline.Map(x => x.Head)),
				HandLeft = _hand.Deserialize(ref reader, baseline.Map(x => x.HandLeft)),
				HandRight = _hand.Deserialize(ref reader, baseline.Map(x => x.HandRight))
			};
		}

		public void Serialize(ref BitPackWriter writer, BodyMessage now, Option<BodyMessage> baseline)
		{
			_head.Serialize(ref writer, now.Head, baseline.Map(x => x.Head));
			_hand.Serialize(ref writer, now.HandLeft, baseline.Map(x => x.HandLeft));
			_hand.Serialize(ref writer, now.HandRight, baseline.Map(x => x.HandRight));
		}
	}
}
