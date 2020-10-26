using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct InputSnapshotMessage : ISerializer, IDeltable<InputSnapshotMessage, InputSnapshotMessage>
	{
		public Option<string> Level;
		public Option<MoveMessage> DeltaMove;

		public InputSnapshotMessage InitialDelta => this;

		public void Deserialize(ref BitPackReader reader)
		{
			Level = reader.GetOption((ref BitPackReader r) => r.Bytes.GetStringWithByteLength());
			DeltaMove = reader.GetOption<MoveMessage>();
		}

		public void Serialize(ref BitPackWriter writer)
		{
			writer.Put(Level, (ref BitPackWriter w, string v) => w.Bytes.PutStringWithByteLength(v));
			writer.Put(DeltaMove);
		}

		public Option<InputSnapshotMessage> CreateDelta(InputSnapshotMessage head)
		{
			var deltas = new DeltaCreator<InputSnapshotMessage>(this, head);

			return Option.Some(new InputSnapshotMessage
			{
				Level = deltas.Create(x => x.Level, x => x.ToEqualityDelta()),
				DeltaMove = deltas.Create(x => x.DeltaMove)
			});
		}

		public InputSnapshotMessage ConsumeDelta(InputSnapshotMessage head)
		{
			var deltas = new DeltaConsumer<InputSnapshotMessage>(this, head);

			return new InputSnapshotMessage
			{
				Level = deltas.Consume(x => x.Level, x => x.ToEqualityDelta(), x => x),
				DeltaMove = deltas.Consume(x => x.DeltaMove)
			};
		}
	}
}
