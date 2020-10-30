using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct LongSerializer : ISerializer<long>
	{
		public long Deserialize(ref BitPackReader reader)
		{
			long conv;

			conv = reader.Bytes.Pop();
			conv |= (long) reader.Bytes.Pop() << 8;
			conv |= (long) reader.Bytes.Pop() << 16;
			conv |= (long) reader.Bytes.Pop() << 24;

			return conv;
		}

		public void Serialize(ref BitPackWriter writer, long value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
			writer.Bytes.Push((byte) (value >> 16));
			writer.Bytes.Push((byte) (value >> 24));
		}
	}
}
