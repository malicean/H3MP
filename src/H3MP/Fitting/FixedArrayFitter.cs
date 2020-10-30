using System;

namespace H3MP.Fitting
{
	public static class FixedArrayFitterExtensions
	{
		public static IFitter<T[]> ToFixedArray<T>(this IFitter<T> @this)
		{
			return new FixedArrayFitter<T>(@this);
		}
	}

	public class FixedArrayFitter<T> : IFitter<T[]>
	{
		private readonly IFitter<T> _fitter;

		public FixedArrayFitter(IFitter<T> fitter)
		{
			_fitter = fitter;
		}

		public T[] Fit(T[] a, T[] b, float t)
		{
			if (a.Length != b.Length)
			{
				throw new FormatException("Array lengts must be equal (fixed).");
			}

			var buffer = new T[a.Length];
			for (var i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = _fitter.Fit(a[i], b[i], t);
			}

			return buffer;
		}
	}
}
