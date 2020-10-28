using H3MP.Utils;

namespace H3MP.Messages
{
	public struct InputSnapshotMessage
	{
		public Option<string> Level;

		public BodyMessage Body;
	}

	public struct DeltaInputSnapshotMessage : IOptionComposite
	{
		public Option<Option<string>> Level;
		public Option<DeltaBodyMessage> Body;

		public bool HasSome => Level.IsSome || Body.IsSome;
    }

	public class InputSnapshotFitter : IFitter<InputSnapshotMessage>
	{
		private IFitter<Option<string>> _level;
		private IFitter<BodyMessage> _body;

		public InputSnapshotMessage Fit(InputSnapshotMessage a, InputSnapshotMessage b, float t)
		{
			var fitter = new SuperFitter<InputSnapshotMessage>(a, b, t);

			fitter.Include(x => x.Level, (ref InputSnapshotMessage body, Option<string> value) => body.Level = value, _level);
			fitter.Include(x => x.Body, (ref InputSnapshotMessage body, BodyMessage value) => body.Body = value, _body);

			return fitter.Body;
		}
	}

	public class DeltaInputSnapshotSerializer : ISerializer<DeltaInputSnapshotMessage>
	{
		private readonly ISerializer<Option<Option<string>>> _level;
		private readonly ISerializer<Option<DeltaBodyMessage>> _body;

		public DeltaInputSnapshotSerializer()
		{
			_level = PrimitiveSerializers.Char.ToString(TruncatedSerializers.ByteAsInt).ToOption().ToOption();
			_body = new DeltaBodyMessageSerializer().ToOption();
		}

		public DeltaInputSnapshotMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaInputSnapshotMessage
			{
				Level = _level.Deserialize(ref reader),
				Body = _body.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaInputSnapshotMessage value)
		{
			_level.Serialize(ref writer, value.Level);
			_body.Serialize(ref writer, value.Body);
		}
	}
}
