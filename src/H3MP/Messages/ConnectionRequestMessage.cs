using H3MP.IO;
using H3MP.Models;
using H3MP.Serialization;

namespace H3MP.Messages
{
	public struct ConnectionRequestMessage
	{
		public bool IsAdmin;

		public Key32 Key;
	}

	public class ConnectionRequestSerializer : ISerializer<ConnectionRequestMessage>
	{
		private readonly ISerializer<bool> _isAdmin;
		private readonly ISerializer<Key32> _key;

		public ConnectionRequestSerializer()
        {
			_isAdmin = PrimitiveSerializers.Bool;
            _key = CustomSerializers.Key32;
        }

        public ConnectionRequestMessage Deserialize(ref BitPackReader reader)
		{
			return new ConnectionRequestMessage
			{
				IsAdmin =_isAdmin.Deserialize(ref reader),
				Key = _key.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, ConnectionRequestMessage value)
		{
			_isAdmin.Serialize(ref writer, value.IsAdmin);
			_key.Serialize(ref writer, value.Key);
        }
	}
}
