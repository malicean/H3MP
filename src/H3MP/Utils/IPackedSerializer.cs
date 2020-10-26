namespace H3MP.Utils
{
	public interface ISerializer<T>
	{
		T Deserialize(ref BitPackReader reader);

		void Serialize(ref BitPackWriter writer, T value);
	}
}
