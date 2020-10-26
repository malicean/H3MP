using System;

namespace H3MP.Utils
{
	public readonly struct ByteSerializer : ISerializer<byte>
	{
		public byte Deserialize(ref BitPackReader reader)
		{
			return reader.Bytes.GetByte();
		}

		public void Serialize(ref BitPackWriter writer, byte value)
		{
			writer.Bytes.Put((int) value);
		}
	}
}
