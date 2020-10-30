using System;
using H3MP.Utils;

namespace H3MP.Fitting
{
	public struct SuperFitter<T> where T : new()
	{
		private readonly T _a;
		private readonly T _b;
		private readonly float _t;

		private T _body;
		public T Body => _body;

		public SuperFitter(T a, T b, float t)
		{
			_a = a;
			_b = b;
			_t = t;

			_body = new T();
		}

		public void Include<TValue>(Func<T, TValue> valueGetter, ChildSetter<T, TValue> valueSetter, IFitter<TValue> fitter)
		{
			valueSetter(ref _body, fitter.Fit(valueGetter(_a), valueGetter(_b), _t));
		}
	}
}
