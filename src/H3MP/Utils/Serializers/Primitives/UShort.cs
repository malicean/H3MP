namespace H3MP.Utils
{
	public readonly struct UShortSerializer : ISerializer<ushort>
	{
		public ushort Deserialize(ref BitPackReader reader)
		{
			ushort value;

			value = reader.Bytes.Pop();
			value |= (ushort) (reader.Bytes.Pop() << 8);

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ushort value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
		}
	}
}
