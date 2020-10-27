namespace H3MP.Utils
{
	public readonly struct UShortUIntConverter : IConverter<ushort, uint>, IConverter<uint, ushort>
	{
		public uint Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(uint value)
		{
			return (ushort) value;
		}
	}
}
