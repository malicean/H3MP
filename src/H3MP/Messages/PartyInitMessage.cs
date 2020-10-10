using System.Net;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP
{
    public struct PartyInitMessage : INetSerializable
    {
        public Key32 ID { get; private set; }

        public byte Size { get; private set; }

        public byte Max { get; private set; }

        public JoinSecret Secret { get; private set; }

        public PartyInitMessage(Key32 id, byte size, byte max, JoinSecret secret)
        {
            ID = id;
            Size = size;
            Max = max;
            Secret = secret;
        }

        public void Deserialize(NetDataReader reader)
        {
            ID = reader.GetKey32();
            Size = reader.GetByte();
            Max = reader.GetByte();
            Secret = reader.GetJoinSecret();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ID);
            writer.Put(Size);
            writer.Put(Max);
            writer.Put(Secret);
        }
    }
}