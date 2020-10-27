namespace H3MP.Utils
{
	public readonly struct IntSerializer<TUIntSerializer> : ISerializer<int> where TUIntSerializer : ISerializer<uint>
	{
		private readonly TUIntSerializer _uint;

		public IntSerializer(TUIntSerializer @uint)
		{
			_uint = @uint;
		}

		public int Deserialize(ref BitPackReader reader)
		{
			return (int) _uint.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, int value)
		{
			_uint.Serialize(ref writer, (uint) value);
		}
	}
}
