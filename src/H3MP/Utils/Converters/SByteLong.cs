namespace H3MP.Utils
{
	public readonly struct SByteLongConverter : IConverter<sbyte, long>, IConverter<long, sbyte>
	{
		public long Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(long value)
		{
			return (sbyte) value;
		}
	}
}
