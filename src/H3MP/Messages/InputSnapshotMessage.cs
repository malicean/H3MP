using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct InputSnapshotMessage : INetSerializable, IDeltable<InputSnapshotMessage>
	{
		public Option<string> Level;
		public Option<MoveMessage> DeltaMove;

        public void Deserialize(NetDataReader reader)
        {
			// Dirty from default.
			reader.GetDirties(
				Level, x => x.GetStringWithByteLength(),
				DeltaMove, x => x.Get<MoveMessage>()
			);
        }

        public void Serialize(NetDataWriter writer)
        {
			writer.PutDirties(
				Level, (NetDataWriter w, string v) => w.PutStringWithByteLength(v),
				DeltaMove, (NetDataWriter w, MoveMessage v) => w.Put(v)
			);
        }

		public InputSnapshotMessage ConsumeDelta(InputSnapshotMessage head)
        {
			Level.Rebase(head.Level);
			DeltaMove.Rebase(head.DeltaMove);

			return head;
        }

        public InputSnapshotMessage CreateDelta(InputSnapshotMessage head)
        {
            var delta = new InputSnapshotMessage
			{
				Level = head.Level.Rebase(Level),
				DeltaMove = head.DeltaMove.Rebase(DeltaMove)
			}

			Level.
        }
    }
}
