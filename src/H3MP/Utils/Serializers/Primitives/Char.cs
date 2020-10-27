namespace H3MP.Utils
{
	public readonly struct CharSerializer<TUShortSerializer> : ISerializer<char> where TUShortSerializer : ISerializer<ushort>
	{
		private readonly TUShortSerializer _ushort;

		public CharSerializer(TUShortSerializer @ushort)
		{
			_ushort = @ushort;
		}

		public char Deserialize(ref BitPackReader reader)
		{
			return (char) _ushort.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, char value)
		{
			_ushort.Serialize(ref writer, (ushort) value);
		}
	}
}
