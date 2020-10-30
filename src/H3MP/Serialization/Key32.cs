using System;
using H3MP.IO;
using H3MP.Models;

namespace H3MP.Serialization
{
    public class Key32Serializer : ISerializer<Key32>
    {
		private readonly ISerializer<byte[]> _bytes;

		public Key32Serializer()
		{
			_bytes = PrimitiveSerializers.Byte.ToArrayFixed(32);
		}

        public Key32 Deserialize(ref BitPackReader reader)
        {
            return Key32.TryFromBytes(_bytes.Deserialize(ref reader), out var key) ? key : throw new NotImplementedException("This should never happen. Code is being phased out anyway.");
        }

        public void Serialize(ref BitPackWriter writer, Key32 value)
        {
            _bytes.Serialize(ref writer, value.Data);
        }
    }
}
