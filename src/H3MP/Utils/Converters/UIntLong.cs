namespace H3MP.Utils
{
	public readonly struct UIntLongConverter : IConverter<uint, long>, IConverter<long, uint>
	{
		public long Convert(uint value)
		{
			return value;
		}

		public uint Convert(long value)
		{
			return (uint) value;
		}
	}
}
