namespace H3MP.Utils
{
	public readonly struct UShortSerializer<TByteSerializer> : ISerializer<ushort> where TByteSerializer : ISerializer<byte>
	{
		private readonly TByteSerializer _byte;

		public UShortSerializer(TByteSerializer @byte)
		{
			_byte = @byte;
		}

		public ushort Deserialize(ref BitPackReader reader)
		{
			ushort value = 0;
			value |= _byte.Deserialize(ref reader);
			value |= (ushort) (_byte.Deserialize(ref reader) << 8);

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ushort value)
		{
			_byte.Serialize(ref writer, (byte) value);
			_byte.Serialize(ref writer, (byte) (value >> 8));
		}
	}
}
