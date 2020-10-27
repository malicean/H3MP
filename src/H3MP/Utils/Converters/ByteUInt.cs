namespace H3MP.Utils
{
	public readonly struct ByteUIntConverter : IConverter<byte, uint>, IConverter<uint, byte>
	{
		public uint Convert(byte value)
		{
			return value;
		}

		public byte Convert(uint value)
		{
			return (byte) value;
		}
	}
}
