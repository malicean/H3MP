namespace H3MP.Utils
{
	public readonly struct ByteULongConverter : IConverter<byte, ulong>, IConverter<ulong, byte>
	{
		public ulong Convert(byte value)
		{
			return value;
		}

		public byte Convert(ulong value)
		{
			return (byte) value;
		}
	}
}
