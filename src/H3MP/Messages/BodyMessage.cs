using System;
using H3MP.Differentiation;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Serialization;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct BodyMessage
	{
		public TransformMessage Root;
		public TransformMessage Head;
		public TransformMessage HandLeft;
		public TransformMessage HandRight;
    }

	public struct DeltaBodyMessage : IOptionComposite
	{
		public Option<TransformMessage> Root;
		public Option<TransformMessage> Head;
		public Option<TransformMessage> HandLeft;
		public Option<TransformMessage> HandRight;

		public bool HasSome => Root.IsSome || Head.IsSome || HandLeft.IsSome || HandRight.IsSome;
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

			fitter.Include(x => x.Root, (ref BodyMessage body, TransformMessage value) => body.Root = value, _transform);
			fitter.Include(x => x.Head, (ref BodyMessage body, TransformMessage value) => body.Head = value, _transform);
			fitter.Include(x => x.HandLeft, (ref BodyMessage body, TransformMessage value) => body.HandLeft = value, _transform);
			fitter.Include(x => x.HandRight, (ref BodyMessage body, TransformMessage value) => body.HandRight = value, _transform);

			return fitter.Body;
		}
	}

	public class BodyMessageDifferentiator : IDifferentiator<BodyMessage, DeltaBodyMessage>
	{
		private readonly TransformMessageDifferentiator _transform;

		public BodyMessageDifferentiator()
		{
			_transform = new TransformMessageDifferentiator();
		}

		public Option<DeltaBodyMessage> CreateDelta(BodyMessage now, Option<BodyMessage> baseline)
		{
			var creator = new SuperDeltaCreator<BodyMessage, DeltaBodyMessage>(now, baseline);

			creator.Include(x => x.Root, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.Root = child, _transform);
			creator.Include(x => x.Head, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.Head = child, _transform);
			creator.Include(x => x.HandLeft, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.HandLeft = child, _transform);
			creator.Include(x => x.HandRight, (ref DeltaBodyMessage delta, Option<TransformMessage> child) => delta.HandRight = child, _transform);

			return creator.Body;
		}

		public BodyMessage ConsumeDelta(DeltaBodyMessage delta, Option<BodyMessage> now)
		{
			var consumer = new SuperDeltaConsumer<DeltaBodyMessage, BodyMessage>(delta, now);

			consumer.Include(x => x.Root, x => x.Root, (ref BodyMessage body, TransformMessage value) => body.Root = value, _transform);
			consumer.Include(x => x.Head, x => x.Head, (ref BodyMessage body, TransformMessage value) => body.Head = value, _transform);
			consumer.Include(x => x.HandLeft, x => x.HandLeft, (ref BodyMessage body, TransformMessage value) => body.HandLeft = value, _transform);
			consumer.Include(x => x.HandRight, x => x.HandRight, (ref BodyMessage body, TransformMessage value) => body.HandRight = value, _transform);

			return consumer.Body;
		}
	}

	public class DeltaBodyMessageSerializer : ISerializer<DeltaBodyMessage>
	{
		private ISerializer<Option<TransformMessage>> _root;
		private ISerializer<Option<TransformMessage>> _child;

		public DeltaBodyMessageSerializer()
		{
			var rot = new SmallestThreeQuaternionSerializer(PackedSerializers.Float(1));

			var rootPos = new Vector3Serializer(PackedSerializers.Float(50)); // In case of sticky jumping
			_root = new TransformMessageSerializer(rootPos, rot).ToOption();

			var childPos = new Vector3Serializer(PackedSerializers.Float(3)); // Accounts for bald eagles and Lebron James
			_child = new TransformMessageSerializer(childPos, rot).ToOption();
		}

		public DeltaBodyMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaBodyMessage
			{
				Root = _root.Deserialize(ref reader),
				Head = _child.Deserialize(ref reader),
				HandLeft = _child.Deserialize(ref reader),
				HandRight = _child.Deserialize(ref reader),
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaBodyMessage value)
		{
			_root.Serialize(ref writer, value.Root);
			_child.Serialize(ref writer, value.Head);
			_child.Serialize(ref writer, value.HandLeft);
			_child.Serialize(ref writer, value.HandRight);
		}
	}
}
