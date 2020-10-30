using H3MP.IO;

namespace H3MP.Serialization
{
    public class BitArraySerializer : ISerializer<BitArray>
    {
		private readonly ISerializer<int> _byteLength;
        private readonly ISerializer<byte> _trailingBits;
		private readonly ISerializer<bool> _bit;

		public BitArraySerializer(ISerializer<int> byteLength, ISerializer<byte> trailingBits, ISerializer<bool> bit)
        {
            _byteLength = byteLength;
            _trailingBits = trailingBits;
            _bit = bit;
        }

        public BitArray Deserialize(ref BitPackReader reader)
        {
            var byteLength = _byteLength.Deserialize(ref reader);
			var trailingBits = _trailingBits.Deserialize(ref reader);

			var array = new BitArray(new byte[byteLength], trailingBits);
			for (int i = 0; i < array.BitLength; ++i)
			{
				if (_bit.Deserialize(ref reader))
				{
					array[i] = true;
				}
			}

			return array;
        }

        public void Serialize(ref BitPackWriter writer, BitArray value)
        {
            _byteLength.Serialize(ref writer, value.Buffer.Length);
			_trailingBits.Serialize(ref writer, value.TrailingBits);

			for (int i = 0; i < value.BitLength; ++i)
			{
				_bit.Serialize(ref writer, value[i]);
			}
        }
    }
}
