using H3MP.Differentiation;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Serialization;
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

		public BodyMessageFitter()
		{
			_transform = new TransformMessageFitter();
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
		private readonly IDifferentiator<TransformMessage, TransformMessage> _transform;

		public BodyMessageDifferentiator(IDifferentiator<TransformMessage, TransformMessage> transform)
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
		private ISerializer<Option<TransformMessage>> _transform;

		public DeltaBodyMessageSerializer()
		{
			// TODO: optimize this (1.0.0 milestone)
			var vec = PrimitiveSerializers.Float.ToVector3();
			var quat = PrimitiveSerializers.Float.ToQuaternion();

			_transform = new TransformMessageSerializer(vec, quat).ToOption();
		}

		public DeltaBodyMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaBodyMessage
			{
				Head = _transform.Deserialize(ref reader),
				HandLeft = _transform.Deserialize(ref reader),
				HandRight = _transform.Deserialize(ref reader),
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaBodyMessage value)
		{
			_transform.Serialize(ref writer, value.Head);
			_transform.Serialize(ref writer, value.HandLeft);
			_transform.Serialize(ref writer, value.HandRight);
		}
	}
}
