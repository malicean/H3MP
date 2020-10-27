namespace H3MP.Utils
{
	public readonly struct UShortLongConverter : IConverter<ushort, long>, IConverter<long, ushort>
	{
		public long Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(long value)
		{
			return (ushort) value;
		}
	}
}
