using H3MP.IO;
using H3MP.Utils;

namespace H3MP.Serialization
{
	public interface IDeltaSerializer<T>
	{
		T Deserialize(ref BitPackReader reader, Option<T> baseline);

		void Serialize(ref BitPackWriter writer, T now, Option<T> baseline);
	}
}
