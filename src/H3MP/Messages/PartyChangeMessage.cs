using DiscordRPC;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP
{
    public struct PartyChangeMessage : INetSerializable
    {
        public int Size { get; private set;  }

        public PartyChangeMessage(int size)
        {
            Size = size;
        }

        public void Deserialize(NetDataReader reader)
        {
            Size = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Size);
        }
    }
}