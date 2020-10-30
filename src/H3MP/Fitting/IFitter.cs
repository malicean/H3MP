namespace H3MP.Fitting
{
	public interface IFitter<T>
	{
		T Fit(T a, T b, float t);
	}
}
