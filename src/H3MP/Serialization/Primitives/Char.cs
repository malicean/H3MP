using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct CharSerializer : ISerializer<char>
	{
		public char Deserialize(ref BitPackReader reader)
		{
			ushort conv;

			conv = reader.Bytes.Pop();
			conv |= (ushort) (reader.Bytes.Pop() << 8);

			return (char) conv;
		}

		public void Serialize(ref BitPackWriter writer, char value)
		{
			var conv = (ushort) value;
			writer.Bytes.Push((byte) (conv));
			writer.Bytes.Push((byte) (conv >> 8));
		}
	}
}
