using H3MP.IO;

namespace H3MP.Serialization
{
	public interface ISerializer<T>
	{
		T Deserialize(ref BitPackReader reader);

		void Serialize(ref BitPackWriter writer, T value);
	}
}
