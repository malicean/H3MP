namespace H3MP.Utils
{
	public readonly struct UShortULongConverter : IConverter<ushort, ulong>, IConverter<ulong, ushort>
	{
		public ulong Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(ulong value)
		{
			return (ushort) value;
		}
	}
}
