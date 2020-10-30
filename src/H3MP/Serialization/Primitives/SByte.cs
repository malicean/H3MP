using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct SByteSerializer : ISerializer<sbyte>
	{
		public sbyte Deserialize(ref BitPackReader reader)
		{
			return (sbyte) reader.Bytes.Pop();
		}

		public void Serialize(ref BitPackWriter writer, sbyte value)
		{
			writer.Bytes.Push((byte) value);
		}
	}
}
