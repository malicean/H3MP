using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
public struct InputSnapshotMessage : IPackedSerializable, IDeltable<InputSnapshotMessage, InputSnapshotMessage>
{
	public Option<string> Level;
	public Option<MoveMessage> DeltaMove;

	public InputSnapshotMessage InitialDelta => this;

	public void Deserialize(BitPackReader reader)
	{
		Level = reader.GetOption((ref BitPackReader r) => r.Bytes.GetStringWithByteLength());
		DeltaMove = reader.GetOption<MoveMessage>();
	}

	public void Serialize(BitPackWriter writer)
	{
		writer.Put(Level, (ref BitPackWriter w, string v) => w.Bytes.PutStringWithByteLength(v));
		writer.Put(DeltaMove);
	}

	public Option<InputSnapshotMessage> CreateDelta(InputSnapshotMessage head)
	{
		var deltas = new DeltaComparer<InputSnapshotMessage>(this, head);
		return Option.Some(new InputSnapshotMessage
		{
			Level = deltas.Create(x => x.Level, x => x.ToEqualityDelta()),
			DeltaMove = deltas.Create(x => x.DeltaMove)
		});
	}

	public InputSnapshotMessage ConsumeDelta(InputSnapshotMessage head)
	{
		var deltas = new DeltaComparer<InputSnapshotMessage>(this, head);
		return new InputSnapshotMessage
		{
			Level = deltas.Consume(x => x.Level, x => x.ToEqualityDelta(), x => x),
			DeltaMove = deltas.Consume(x => x.DeltaMove)
		};
	}
}
}
