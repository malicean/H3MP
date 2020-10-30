using System.Net;
using H3MP.IO;

namespace H3MP.Serialization
{
    public class IPEndPointSerializer : ISerializer<IPEndPoint>
    {
		private readonly ISerializer<IPAddress> _address;
		private readonly ISerializer<ushort> _port;

		public IPEndPointSerializer()
		{
			_address = SystemSerializers.IPAddress;
			_port = PrimitiveSerializers.UShort;
		}

        public IPEndPoint Deserialize(ref BitPackReader reader)
        {
			var address = _address.Deserialize(ref reader);
			var port = _port.Deserialize(ref reader);

            return new IPEndPoint(address, port);
        }

        public void Serialize(ref BitPackWriter writer, IPEndPoint value)
        {
            _address.Serialize(ref writer, value.Address);
			_port.Serialize(ref writer, (ushort) value.Port);
        }
    }
}
