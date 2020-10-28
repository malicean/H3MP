namespace H3MP.Utils
{
	public interface IDeltaSerializer<T>
	{
		T Deserialize(ref BitPackReader reader, Option<T> baseline);

		void Serialize(ref BitPackWriter writer, T delta, Option<T> now);
	}
}
