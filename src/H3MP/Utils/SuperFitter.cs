using System;

namespace H3MP.Utils
{
	public readonly struct SuperFitter<T>
	{
		private readonly T _a;
		private readonly T _b;
		private readonly float _t;

		public SuperFitter(T a, T b, float t)
		{
			_a = a;
			_b = b;
			_t = t;
		}

		public TValue Fit<TValue, TFitter>(Func<T, TValue> valueOf, TFitter fitter) where TFitter : IFitter<TValue>
		{
			return fitter.Fit(valueOf(_a), valueOf(_b), _t);
		}
	}
}
