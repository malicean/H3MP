using H3MP.Networking.Extensions;
using LiteNetLib.Utils;
using System;

namespace H3MP.Networking
{
	public struct MismatchedVersionErrorMessage : INetSerializable
	{
		public Version ServerVersion { get; private set; }

		public Version ClientVersion { get; private set; }

		public MismatchedVersionErrorMessage(Version serverVersion, Version clientVersion)
		{
			ServerVersion = serverVersion;
			ClientVersion = clientVersion;
		}

		public void Deserialize(NetDataReader reader)
		{
			ServerVersion = reader.GetVersion();
			ClientVersion = reader.GetVersion();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ServerVersion);
			writer.Put(ClientVersion);
		}
	}
}
