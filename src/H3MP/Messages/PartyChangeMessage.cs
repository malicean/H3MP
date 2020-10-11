using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct PartyChangeMessage : INetSerializable
    {
        public int CurrentSize { get; private set;  }

        public PartyChangeMessage(int currentSize)
        {
            CurrentSize = currentSize;
        }

        public void Deserialize(NetDataReader reader)
        {
            CurrentSize = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte) CurrentSize);
        }
    }
}