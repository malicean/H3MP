namespace H3MP.Utils
{
	public readonly struct IntLongConverter : IConverter<int, long>, IConverter<long, int>
	{
		public long Convert(int value)
		{
			return value;
		}

		public int Convert(long value)
		{
			return (int) value;
		}
	}
}
