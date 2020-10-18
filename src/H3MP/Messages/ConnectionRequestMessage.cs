using H3MP.Models;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct ConnectionRequestMessage : INetSerializable
	{
		public Key32 AccessKey { get; private set; }

		public Key32? HostKey { get; private set; }

		public double ClientTime { get; private set; }

		public ConnectionRequestMessage(Key32 accessKey, Key32? hostKey = null)
		{
			AccessKey = accessKey;
			HostKey = hostKey;
			ClientTime = LocalTime.Now;
		}

		public void Deserialize(NetDataReader reader)
		{
			AccessKey = reader.GetKey32();
			HostKey = reader.GetNullable<Key32>(NetDataReaderExtensions.GetKey32);
			ClientTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(AccessKey);
			writer.Put(HostKey, NetDataWriterExtensions.Put);
			writer.Put(ClientTime);
		}
	}
}
