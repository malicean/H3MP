namespace H3MP.Fitting
{
	public class ThresholdFitter<T> : IFitter<T>
	{
		private readonly float _threshold;

		public ThresholdFitter(float threshold)
		{
			_threshold = threshold;
		}

		public T Fit(T a, T b, float t)
		{
			return t < _threshold ? a : b;
		}
	}
}
