namespace H3MP.Utils
{
	public readonly struct ByteIntConverter : IConverter<byte, int>, IConverter<int, byte>
	{
		public int Convert(byte value)
		{
			return value;
		}

		public byte Convert(int value)
		{
			return (byte) value;
		}
	}
}
