using System.Net;
using H3MP.IO;

namespace H3MP.Serialization
{
    public class IPAddressSerializer : ISerializer<IPAddress>
    {
		private readonly ISerializer<byte[]> _bytes;

		public IPAddressSerializer()
		{
			_bytes = PrimitiveSerializers.Byte.ToArrayDynamic(TruncatedSerializers.ByteAsInt);
		}

        public IPAddress Deserialize(ref BitPackReader reader)
        {
			return new IPAddress(_bytes.Deserialize(ref reader));
        }

        public void Serialize(ref BitPackWriter writer, IPAddress value)
        {
            _bytes.Serialize(ref writer, value.GetAddressBytes());
        }
    }
}
