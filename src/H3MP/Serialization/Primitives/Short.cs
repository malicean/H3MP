using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct ShortSerializer : ISerializer<short>
	{
		public short Deserialize(ref BitPackReader reader)
		{
			short value;

			value = reader.Bytes.Pop();
			value |= (short) (reader.Bytes.Pop() << 8);

			return value;
		}

		public void Serialize(ref BitPackWriter writer, short value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
		}
	}
}
