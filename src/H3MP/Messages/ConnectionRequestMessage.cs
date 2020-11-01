using System;
using H3MP.IO;
using H3MP.Models;
using H3MP.Serialization;

namespace H3MP.Messages
{
	public readonly struct ConnectionRequestMessage
	{
		public readonly bool IsAdmin;
		public readonly Key32 Key;
		public readonly Version Version;

		public ConnectionRequestMessage(bool isAdmin, Key32 key, Version version)
		{
			IsAdmin = isAdmin;
			Key = key;
			Version = version;
		}
	}

	public class ConnectionRequestSerializer : ISerializer<ConnectionRequestMessage>
	{
		private readonly ISerializer<bool> _isAdmin;
		private readonly ISerializer<Key32> _key;
		private readonly ISerializer<Version> _version;

		public ConnectionRequestSerializer()
        {
			_isAdmin = PrimitiveSerializers.Bool;
            _key = CustomSerializers.Key32;
			_version = SystemSerializers.Version;
        }

        public ConnectionRequestMessage Deserialize(ref BitPackReader reader)
		{
			return new ConnectionRequestMessage(
				_isAdmin.Deserialize(ref reader),
				_key.Deserialize(ref reader),
				_version.Deserialize(ref reader)
			);
		}

		public void Serialize(ref BitPackWriter writer, ConnectionRequestMessage value)
		{
			_isAdmin.Serialize(ref writer, value.IsAdmin);
			_key.Serialize(ref writer, value.Key);
			_version.Serialize(ref writer, value.Version);
        }
	}
}
