namespace H3MP.Utils
{
	public readonly struct ByteLongConverter : IConverter<byte, long>, IConverter<long, byte>
	{
		public long Convert(byte value)
		{
			return value;
		}

		public byte Convert(long value)
		{
			return (byte) value;
		}
	}
}
