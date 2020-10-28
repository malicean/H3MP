namespace H3MP.Utils
{
	public interface IFitter<T>
	{
		T Fit(T a, T b, float t);
	}
}
