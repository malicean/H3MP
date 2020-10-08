using H3MP.Utils;
using LiteNetLib.Utils;
using System;

namespace H3MP.Messages
{
	public struct ConnectionRequestMessage : INetSerializable
	{
		public ConnectionKey Key { get; private set; }

		public double ClientTime { get; private set; }

		public ConnectionRequestMessage(ConnectionKey key)
		{
			Key = key;
			ClientTime = LocalTime.Now;
		}

		public void Deserialize(NetDataReader reader)
		{
			Key = reader.GetConnectionKey();
			ClientTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Key);
			writer.Put(ClientTime);
		}
	}
}
