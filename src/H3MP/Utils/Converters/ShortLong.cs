namespace H3MP.Utils
{
	public readonly struct ShortLongConverter : IConverter<short, long>, IConverter<long, short>
	{
		public long Convert(short value)
		{
			return value;
		}

		public short Convert(long value)
		{
			return (short) value;
		}
	}
}
