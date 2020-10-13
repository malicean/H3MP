using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct PlayerJoinMessage : INetSerializable
    {
        public byte ID { get; private set; }

        public Timestamped<PlayerTransformsMessage> Transforms { get; private set; }

        public PlayerJoinMessage(byte id, Timestamped<PlayerTransformsMessage> transforms)
        {
            ID = id;
            Transforms = transforms;
        }

        public void Deserialize(NetDataReader reader)
        {
            ID = reader.GetByte();
            Transforms = reader.Get<Timestamped<PlayerTransformsMessage>>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ID);
            writer.Put(Transforms);
        }
    }
}