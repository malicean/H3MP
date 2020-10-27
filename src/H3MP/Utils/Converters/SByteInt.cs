namespace H3MP.Utils
{
	public readonly struct SByteIntConverter : IConverter<sbyte, int>, IConverter<int, sbyte>
	{
		public int Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(int value)
		{
			return (sbyte) value;
		}
	}
}
