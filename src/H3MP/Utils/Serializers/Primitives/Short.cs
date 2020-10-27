namespace H3MP.Utils
{
	public readonly struct ShortSerializer<TUShortSerializer> : ISerializer<short> where TUShortSerializer : ISerializer<ushort>
	{
		private readonly TUShortSerializer _ushort;

		public ShortSerializer(TUShortSerializer @ushort)
		{
			_ushort = @ushort;
		}

		public short Deserialize(ref BitPackReader reader)
		{
			return (short) _ushort.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, short value)
		{
			_ushort.Serialize(ref writer, (ushort) value);
		}
	}
}
