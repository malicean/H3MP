namespace H3MP.Utils
{
	public readonly struct LongLongConverter : IConverter<long, long>
	{
		public long Convert(long value)
		{
			return value;
		}
	}
}
