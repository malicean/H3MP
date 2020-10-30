using H3MP.Utils;

namespace H3MP.Fitting
{
	public static class OptionFitterExtensions
	{
		public static IFitter<Option<T>> ToOption<T>(this IFitter<T> @this)
		{
			return new OptionFitter<T>(@this);
		}
	}

	public class OptionFitter<T> : IFitter<Option<T>>
	{
		private readonly IFitter<T> _fitter;

		public OptionFitter(IFitter<T> fitter)
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
