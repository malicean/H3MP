namespace H3MP.Utils
{
	public interface IConverter<T1, T2>
	{
		T2 Convert(T1 value);
		
		T1 Convert(T2 value);
	}
}
