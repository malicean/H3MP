using H3MP.Utils;

namespace H3MP.Messages
{
	public struct InputSnapshotMessage
	{
		public Option<string> Level;

		public BodyMessage Body;
	}

	public class InputSnapshotFitter : IFitter<InputSnapshotMessage>
	{
		private IFitter<Option<string>> _level;
		private IFitter<BodyMessage> _velocity;

		public InputSnapshotMessage Fit(InputSnapshotMessage a, InputSnapshotMessage b, float t)
		{
			var fitter = new SuperFitter<InputSnapshotMessage>(a, b, t);

			return new InputSnapshotMessage
			{
				Level = fitter.Fit(x => x.Level),
				Body = fitter.Fit(x => x.Body, Fitter)
			};
		}
	}

	public class InputSnapshotSerializer : IDeltaSerializer<InputSnapshotMessage>
	{
		private readonly IDeltaSerializer<Option<string>> _level;
		private readonly IDeltaSerializer<BodyMessage> _velocity;

		public InputSnapshotSerializer()
		{
			_level = EqualityDifferentiator<string>.Instance.ToOption().ToSerializer(PrimitiveSerializers.Char.ToString(TruncatedSerializers.ByteAsInt).ToOption());
			_velocity = new MoveMessageDeltaSerializer();
		}

		public InputSnapshotMessage Deserialize(ref BitPackReader reader, Option<InputSnapshotMessage> baseline)
		{
			return new InputSnapshotMessage
			{
				Level = _level.Deserialize(ref reader, baseline.Map(x => x.Level)),
				Body = _velocity.Deserialize(ref reader, baseline.Map(x => x.Body))
			};
		}

		public void Serialize(ref BitPackWriter writer, InputSnapshotMessage value, Option<InputSnapshotMessage> baseline)
		{
			throw new System.NotImplementedException();
		}
	}
}
