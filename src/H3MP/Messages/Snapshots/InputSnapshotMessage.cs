using System;
using H3MP.Differentiation;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Serialization;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct InputSnapshotMessage : ICopyable<InputSnapshotMessage>
	{
		public Option<string> Level;

		public BodyMessage Body;

		public InputSnapshotMessage Copy()
		{
			var copy = this;

			return copy;
		}
	}

	public struct DeltaInputSnapshotMessage : IOptionComposite
	{
		public Option<Option<string>> Level;
		public Option<DeltaBodyMessage> BodyVelocity;

		public bool HasSome => Level.IsSome || BodyVelocity.IsSome;
    }

	public class InputSnapshotMessageFitter : IFitter<InputSnapshotMessage>
	{
		private IFitter<Option<string>> _level;
		private BodyMessageFitter _bodyVelocity;

		public InputSnapshotMessageFitter()
		{
			_level = Fitters.Oldest<string>().ToOption();
			_bodyVelocity = new BodyMessageFitter();
		}

		public InputSnapshotMessage Fit(InputSnapshotMessage a, InputSnapshotMessage b, float t)
		{
			var fitter = new SuperFitter<InputSnapshotMessage>(a, b, t);

			fitter.Include(x => x.Level, (ref InputSnapshotMessage body, Option<string> value) => body.Level = value, _level);
			fitter.Include(x => x.Body, (ref InputSnapshotMessage body, BodyMessage value) => body.Body = value, _bodyVelocity);

			return fitter.Body;
		}
	}

	public class InputSnapshotMessageDifferentiator : IDifferentiator<InputSnapshotMessage, DeltaInputSnapshotMessage>
	{
		private readonly IDifferentiator<Option<string>, Option<string>> _level;
		private readonly BodyMessageDifferentiator _bodyVelocity;

		public InputSnapshotMessageDifferentiator()
		{
			_level = Differentiators.Equality<string>().ToOption();
			_bodyVelocity = new BodyMessageDifferentiator(Differentiators.Equality<TransformMessage>());
		}

		public InputSnapshotMessage ConsumeDelta(DeltaInputSnapshotMessage delta, Option<InputSnapshotMessage> now)
		{
			var consumer = new SuperDeltaConsumer<DeltaInputSnapshotMessage, InputSnapshotMessage>(delta, now);

			consumer.Include(x => x.Level, x => x.Level, (ref InputSnapshotMessage body, Option<string> value) => body.Level = value, _level);
			consumer.Include(x => x.BodyVelocity, x => x.Body, (ref InputSnapshotMessage body, BodyMessage value) => body.Body = value, _bodyVelocity);

			return consumer.Body;
		}

		public Option<DeltaInputSnapshotMessage> CreateDelta(InputSnapshotMessage now, Option<InputSnapshotMessage> baseline)
		{
			var creator = new SuperDeltaCreator<InputSnapshotMessage, DeltaInputSnapshotMessage>(now, baseline);

			creator.Include(x => x.Level, (ref DeltaInputSnapshotMessage body, Option<Option<string>> value) => body.Level = value, _level);
			creator.Include(x => x.Body, (ref DeltaInputSnapshotMessage body, Option<DeltaBodyMessage> value) => body.BodyVelocity = value, _bodyVelocity);

			return creator.Body;
		}
	}

	public class DeltaInputSnapshotSerializer : ISerializer<DeltaInputSnapshotMessage>
	{
		private readonly ISerializer<Option<Option<string>>> _level;
		private readonly ISerializer<Option<DeltaBodyMessage>> _bodyVelocity;

		public DeltaInputSnapshotSerializer()
		{
			_level = PrimitiveSerializers.Char.ToString(TruncatedSerializers.ByteAsInt).ToOption().ToOption();
			_bodyVelocity = new DeltaBodyMessageSerializer().ToOption();
		}

		public DeltaInputSnapshotMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaInputSnapshotMessage
			{
				Level = _level.Deserialize(ref reader),
				BodyVelocity = _bodyVelocity.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaInputSnapshotMessage value)
		{
			_level.Serialize(ref writer, value.Level);
			_bodyVelocity.Serialize(ref writer, value.BodyVelocity);
		}
	}
}
