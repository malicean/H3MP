using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct UIntSerializer : ISerializer<uint>
	{
		public uint Deserialize(ref BitPackReader reader)
		{
			uint value;

			value = reader.Bytes.Pop();
			value |= (uint) reader.Bytes.Pop() << 8;
			value |= (uint) reader.Bytes.Pop() << 16;
			value |= (uint) reader.Bytes.Pop() << 24;

			return value;
		}

		public void Serialize(ref BitPackWriter writer, uint value)
		{
			writer.Bytes.Push((byte) value);
			writer.Bytes.Push((byte) (value >> 8));
			writer.Bytes.Push((byte) (value >> 16));
			writer.Bytes.Push((byte) (value >> 24));
		}
	}
}
