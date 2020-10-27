namespace H3MP.Utils
{
	public readonly struct ShortIntConverter : IConverter<short, int>, IConverter<int, short>
	{
		public int Convert(short value)
		{
			return value;
		}

		public short Convert(int value)
		{
			return (short) value;
		}
	}
}
