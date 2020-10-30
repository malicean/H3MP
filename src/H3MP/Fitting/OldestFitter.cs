namespace H3MP.Fitting
{
	public class OldestFitter<T> : IFitter<T>
	{
		public static OldestFitter<T> Instance { get; } = new OldestFitter<T>();

		public T Fit(T a, T b, float t)
		{
			return a;
		}
	}
}
