namespace H3MP.Utils
{
	public readonly struct ByteUShortConverter : IConverter<byte, ushort>, IConverter<ushort, byte>
	{
		public ushort Convert(byte value)
		{
			return value;
		}

		public byte Convert(ushort value)
		{
			return (byte) value;
		}
	}
}
