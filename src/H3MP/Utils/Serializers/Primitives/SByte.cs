namespace H3MP.Utils
{
	public readonly struct SByteSerializer<TByteSerializer> : ISerializer<sbyte> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public SByteSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public sbyte Deserialize(ref BitPackReader reader)
		{
			return (sbyte) _byte.Deserialize(ref reader);
		}

		public void Serialize(ref BitPackWriter writer, sbyte value)
		{
			_byte.Serialize(ref writer, (byte) value);
		}
	}
}
