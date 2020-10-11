using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct ConnectionRequestMessage : INetSerializable
	{
		public Key32 Key { get; private set; }

		public double ClientTime { get; private set; }

		public ConnectionRequestMessage(Key32 key)
		{
			Key = key;
			ClientTime = LocalTime.Now;
		}

		public void Deserialize(NetDataReader reader)
		{
			Key = reader.GetKey32();
			ClientTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Key);
			writer.Put(ClientTime);
		}
	}
}
