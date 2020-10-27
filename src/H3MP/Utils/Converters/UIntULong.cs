namespace H3MP.Utils
{
	public readonly struct UIntULongConverter : IConverter<uint, ulong>, IConverter<ulong, uint>
	{
		public ulong Convert(uint value)
		{
			return value;
		}

		public uint Convert(ulong value)
		{
			return (uint) value;
		}
	}
}
