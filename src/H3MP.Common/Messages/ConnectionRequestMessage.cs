using LiteNetLib.Utils;

namespace H3MP.Common.Messages
{
	public struct ConnectionRequestMessage : INetSerializable
	{
		public ushort ApiVersion { get; private set; }

		public string Passphrase { get; private set; }

		public ConnectionRequestMessage(string passphrase)
		{
			ApiVersion = ApiConstants.VERSION;
			Passphrase = passphrase;
		}

		public void Deserialize(NetDataReader reader)
		{
			ApiVersion = reader.GetUShort();
			Passphrase = reader.GetString();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ApiVersion);
			writer.Put(Passphrase);
		}
	}
}
