namespace H3MP.Utils
{
	public interface IConverter<in T1, out T2>
	{
		T2 Convert(T1 value);
	}
}
