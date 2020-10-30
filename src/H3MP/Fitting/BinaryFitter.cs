namespace H3MP.Fitting
{
	public class BinaryFitter<T> : IFitter<T>
	{
		public static BinaryFitter<T> Instance { get; } = new BinaryFitter<T>();

		public T Fit(T a, T b, float t)
		{
			return t < 0.5f ? a : b;
		}
	}
}
