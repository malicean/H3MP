using LiteNetLib.Utils;

namespace H3MP.Utils
{
	public class NetToPackedSerializable<T> : ISerializer<T> where T : INetSerializable, new()
	{
		public T Deserialize(ref BitPackReader reader)
		{
			return reader.Bytes.Get<T>();
		}

		public void Serialize(ref BitPackWriter writer, T value)
		{
			writer.Bytes.Put(value);
		}
	}
}
