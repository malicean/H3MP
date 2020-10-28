namespace H3MP.Utils
{
	public readonly struct ULongSerializer : ISerializer<ulong>
	{
		public ulong Deserialize(ref BitPackReader reader)
		{
			ulong value;

			value = reader.Bytes.Pop();
			value |= (ulong) reader.Bytes.Pop() << 8;
			value |= (ulong) reader.Bytes.Pop() << 16;
			value |= (ulong) reader.Bytes.Pop() << 24;
			value |= (ulong) reader.Bytes.Pop() << 32;
			value |= (ulong) reader.Bytes.Pop() << 40;
			value |= (ulong) reader.Bytes.Pop() << 48;
			value |= (ulong) reader.Bytes.Pop() << 56;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, ulong value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
			writer.Bytes.Push((byte) (value >> 16));
			writer.Bytes.Push((byte) (value >> 24));
			writer.Bytes.Push((byte) (value >> 32));
			writer.Bytes.Push((byte) (value >> 40));
			writer.Bytes.Push((byte) (value >> 48));
			writer.Bytes.Push((byte) (value >> 56));
		}
	}
}
