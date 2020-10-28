namespace H3MP.Utils
{
	public class OptionFitter<T, TFitter> : IFitter<Option<T>> where TFitter : IFitter<T>
	{
		private readonly TFitter _fitter;

		public OptionFitter(TFitter fitter)
		{
			_fitter = fitter;
		}

		public Option<T> Fit(Option<T> a, Option<T> b, float t)
		{
			return a.MatchSome(out var aValue) && b.MatchSome(out var bValue)
				? Option.Some(_fitter.Fit(aValue, bValue, t))
				: Option.None<T>();
		}
	}
}
