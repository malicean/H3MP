using H3MP.Extensions;
using H3MP.IO;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct ConnectionRequestMessage
	{
		public Key32 AccessKey;

		public Option<Key32> HostKey;

		public double ClientTime;
	}

	public class ConnectionRequestSerializer : ISerializer<ConnectionRequestMessage>
	{
		private readonly ISerializer<Key32> _accessKey;
		private readonly ISerializer<Option<Key32>> _hostKey;
		private readonly ISerializer<double> _clientTime;

		public ConnectionRequestSerializer()
        {
            _accessKey = CustomSerializers.Key32;
			_hostKey = CustomSerializers.Key32.ToOption();
			_clientTime = PrimitiveSerializers.Double;
        }

        public ConnectionRequestMessage Deserialize(ref BitPackReader reader)
		{
			return new ConnectionRequestMessage
			{
				AccessKey = _accessKey.Deserialize(ref reader),
				HostKey =_hostKey.Deserialize(ref reader),
				ClientTime =_clientTime.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, ConnectionRequestMessage value)
		{
			_accessKey.Serialize(ref writer, value.AccessKey);
            _hostKey.Serialize(ref writer, value.HostKey);
            _clientTime.Serialize(ref writer, value.ClientTime);
        }
	}
}
