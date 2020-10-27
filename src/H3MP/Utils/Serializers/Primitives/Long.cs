namespace H3MP.Utils
{
	public readonly struct LongSerializer<TULongSerializer> : ISerializer<long> where TULongSerializer : ISerializer<ulong>
	{
		private readonly TULongSerializer _ulong;

		public LongSerializer(TULongSerializer @ulong)
		{
			_ulong = @ulong;
		}

		public long Deserialize(ref BitPackReader reader)
		{
			return (long) _ulong.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, long value)
		{
			_ulong.Serialize(ref writer, (ulong) value);
		}
	}
}
