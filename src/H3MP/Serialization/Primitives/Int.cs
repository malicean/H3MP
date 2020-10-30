using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct IntSerializer : ISerializer<int>
	{
		public int Deserialize(ref BitPackReader reader)
		{
			int value;

			value = reader.Bytes.Pop();
			value |= reader.Bytes.Pop() << 8;
			value |= reader.Bytes.Pop() << 16;
			value |= reader.Bytes.Pop() << 24;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, int value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
			writer.Bytes.Push((byte) (value >> 16));
			writer.Bytes.Push((byte) (value >> 24));
		}
	}
}
