namespace H3MP.Utils
{
	public readonly struct BoolSerializer : ISerializer<bool>
	{
		public bool Deserialize(ref BitPackReader reader)
		{
			return reader.Bits.Pop();
		}

		public void Serialize(ref BitPackWriter writer, bool value)
		{
			writer.Bits.Push(value);
		}
	}
}
