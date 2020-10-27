namespace H3MP.Utils
{
	public readonly struct UIntSerializer<TByteSerializer> : ISerializer<uint> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public UIntSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public uint Deserialize(ref BitPackReader reader)
		{
			uint value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (uint) _byte.Deserialize(ref reader) << 8;
			value |= (uint) _byte.Deserialize(ref reader) << 16;
			value |= (uint) _byte.Deserialize(ref reader) << 24;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, uint value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
			_byte.Serialize(ref writer, (byte) (value >> 16));
			_byte.Serialize(ref writer, (byte) (value >> 24));
		}
	}
}
