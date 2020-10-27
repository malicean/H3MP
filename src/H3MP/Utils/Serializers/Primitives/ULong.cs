namespace H3MP.Utils
{
	public readonly struct ULongSerializer<TByteSerializer> : ISerializer<ulong> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public ULongSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public ulong Deserialize(ref BitPackReader reader)
		{
			ulong value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (ulong) _byte.Deserialize(ref reader) << 8;
			value |= (ulong) _byte.Deserialize(ref reader) << 16;
			value |= (ulong) _byte.Deserialize(ref reader) << 24;
			value |= (ulong) _byte.Deserialize(ref reader) << 32;
			value |= (ulong) _byte.Deserialize(ref reader) << 40;
			value |= (ulong) _byte.Deserialize(ref reader) << 48;
			value |= (ulong) _byte.Deserialize(ref reader) << 56;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ulong value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
			_byte.Serialize(ref writer, (byte) (value >> 16));
			_byte.Serialize(ref writer, (byte) (value >> 24));
			_byte.Serialize(ref writer, (byte) (value >> 32));
			_byte.Serialize(ref writer, (byte) (value >> 40));
			_byte.Serialize(ref writer, (byte) (value >> 48));
			_byte.Serialize(ref writer, (byte) (value >> 56));
		}
	}
}
