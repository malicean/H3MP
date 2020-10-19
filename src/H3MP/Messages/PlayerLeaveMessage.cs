using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct PlayerLeaveMessage : INetSerializable
	{
		public byte ID { get; private set; }

		public PlayerLeaveMessage(byte id)
		{
			ID = id;
		}

		public void Deserialize(NetDataReader reader)
		{
			ID = reader.GetByte();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ID);
		}
	}
}
