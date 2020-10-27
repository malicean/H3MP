namespace H3MP.Utils
{
	public readonly struct UShortIntConverter : IConverter<ushort, int>, IConverter<int, ushort>
	{
		public int Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(int value)
		{
			return (ushort) value;
		}
	}
}
