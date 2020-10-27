namespace H3MP.Utils
{
	public readonly struct SByteShortConverter : IConverter<sbyte, short>, IConverter<short, sbyte>
	{
		public short Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(short value)
		{
			return (sbyte) value;
		}
	}
}
