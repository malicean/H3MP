using System;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct BodyMessage
	{
		public TransformMessage Head;
		public TransformMessage HandLeft;
		public TransformMessage HandRight;
    }

	public struct DeltaBodyMessage : IOptionComposite
	{
		public Option<TransformMessage> Head;
		public Option<TransformMessage> HandLeft;
		public Option<TransformMessage> HandRight;

		public bool HasSome => Head.IsSome || HandLeft.IsSome || HandRight.IsSome;
    }

	public class BodyMessageFitter : IFitter<BodyMessage>
	{
		private readonly TransformMessageFitter _transform;

		public BodyMessageFitter(TransformMessageFitter transform)
		{
			_transform = transform;
		}

		public BodyMessage Fit(BodyMessage a, BodyMessage b, float t)
		{
			var fitter = new SuperFitter<BodyMessage>(a, b, t);

			fitter.Include(x => x.Head, (ref BodyMessage body, TransformMessage value) => body.Head = value, _transform);
			fitter.Include(x => x.HandLeft, (ref BodyMessage body, TransformMessage value) => body.HandLeft = value, _transform);
			fitter.Include(x => x.HandRight, (ref BodyMessage body, TransformMessage value) => body.HandRight = value, _transform);

			return fitter.Body;
		}
	}

	public class BodyMessageDifferentiator : IDifferentiator<BodyMessage, DeltaBodyMessage>
	{
		private TransformMessageDifferentiator _transform;

		public BodyMessageDifferentiator(TransformMessageDifferentiator transform)
		{
			_transform = transform;
		}

		public Option<DeltaBodyMessage> CreateDelta(BodyMessage now, Option<BodyMessage> baseline)
		{
			var creator = new SuperDeltaCreator<BodyMessage, DeltaBodyMessage>(now, baseline);

			creator.Include(x => x.Head, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.Head = child, _transform);
			creator.Include(x => x.HandLeft, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.HandLeft = child, _transform);
			creator.Include(x => x.HandRight, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.HandRight = child, _transform);

			return creator.Body;
		}

		public BodyMessage ConsumeDelta(DeltaBodyMessage delta, Option<BodyMessage> now)
		{
			var consumer = new SuperDeltaConsumer<DeltaBodyMessage, BodyMessage>(delta, now);

			consumer.Include(x => x.Head, x => x.Head, (ref BodyMessage body, TransformMessage value) => body.Head = value, _transform);
			consumer.Include(x => x.HandLeft, x => x.HandLeft, (ref BodyMessage body, TransformMessage value) => body.HandLeft = value, _transform);
			consumer.Include(x => x.HandRight, x => x.HandRight, (ref BodyMessage body, TransformMessage value) => body.HandRight = value, _transform);

			return consumer.Body;
		}
	}

	public class DeltaBodyMessageSerializer : ISerializer<DeltaBodyMessage>
	{
		private ISerializer<Option<TransformMessage>> _head;
		private ISerializer<Option<TransformMessage>> _hand;

		public DeltaBodyMessageSerializer()
		{
			var rot = new SmallestThreeQuaternionSerializer(PackedSerializers.UFloat(1));

			var headPos = new Vector3Serializer(PackedSerializers.Float(50)); // In case of sticky jumping
			_head = new TransformMessageSerializer(headPos, rot).ToOption();

			var handPos = new Vector3Serializer(PackedSerializers.Float(2)); // Accounts for bald eagles
			_hand = new TransformMessageSerializer(handPos, rot).ToOption();
		}

		public DeltaBodyMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaBodyMessage
			{
				Head = _head.Deserialize(ref reader),
				HandLeft = _hand.Deserialize(ref reader),
				HandRight = _hand.Deserialize(ref reader),
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaBodyMessage value)
		{
			_head.Serialize(ref writer, value.Head);
			_hand.Serialize(ref writer, value.HandLeft);
			_hand.Serialize(ref writer, value.HandRight);
		}
	}
}
