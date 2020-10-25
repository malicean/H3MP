using System;

namespace H3MP.Utils
{
	public readonly struct FitCreator<T>
	{
		private readonly T _a;
		private readonly T _b;
		private readonly double _t;

		public FitCreator(T a, T b, double t)
		{
			_a = a;
			_b = b;
			_t = t;
		}

		public TFittable Fit<TFittable>(Func<T, TFittable> fittableOf) where TFittable : ILinearFittable<TFittable>
		{
			return fittableOf(_a).Fit(fittableOf(_b), _t);
		}

		public Option<TFittable> Fit<TFittable>(Func<T, Option<TFittable>> fittableOf) where TFittable : ILinearFittable<TFittable>
		{
			return fittableOf(_a).ToFittable().Fit(fittableOf(_b).ToFittable(), _t);
		}

		public TValue Fit<TFittable, TValue>(Func<T, TFittable> fittableOf, Func<TFittable, TValue> toValue) where TFittable : ILinearFittable<TFittable>
		{
			return toValue(fittableOf(_a).Fit(fittableOf(_b), _t));
		}

		public Option<TValue> Fit<TFittable, TValue>(Func<T, Option<TValue>> valueOf, Func<TValue, TFittable> toFittable, Func<TFittable, TValue> toValue) where TFittable : ILinearFittable<TFittable>
		{
			return valueOf(_a).Map(toFittable).ToFittable().Fit(valueOf(_b).Map(toFittable).ToFittable(), _t).Value.Map(toValue);
		}
	}
}
