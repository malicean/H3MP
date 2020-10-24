using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
public struct InputSnapshotMessage : INetSerializable, IDeltable<InputSnapshotMessage, InputSnapshotMessage>
{
	public Option<string> Level;

	public Option<MoveMessage> DeltaMove;

	public InputSnapshotMessage InitialDelta => this;

	public void Deserialize(NetDataReader reader)
	{
		var options = new NetOptionReader(reader);

		Level = options.Get(r => r.GetStringWithByteLength());
		DeltaMove = options.Get<MoveMessage>();
	}

	public void Serialize(NetDataWriter writer)
	{
		using (var options = new NetOptionWriter(writer))
		{
			options.Put(Level, (w, v) => writer.PutStringWithByteLength(v));
			options.Put(DeltaMove);
		}
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
			Level = deltas.Consume(x => x.Level, x => x.ToEqualityDelta()),
			DeltaMove = deltas.Consume(x => x.DeltaMove)
		};
	}
}
}
