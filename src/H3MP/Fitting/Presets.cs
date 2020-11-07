using UnityEngine;

namespace H3MP.Fitting
{
	public static class Fitters
	{
		public static IFitter<Quaternion> Quaternion { get; } = new QuaternionFitter();
		public static IFitter<Vector3> Vector3 { get; } = new Vector3Fitter();

		public static IFitter<T> Threshold<T>(float threshold)
		{
			return new ThresholdFitter<T>(threshold);
		}

		public static IFitter<T> Oldest<T>()
		{
			return OldestFitter<T>.Instance;
		}
	}

	public static class InverseFitters
	{
		public static IInverseFitter<long> Long { get; } = new LongInverseFitter();
		public static IInverseFitter<uint> UInt { get; } = new UIntInverseFitter();
		public static IInverseFitter<double> Double { get; } = new DoubleInverseFitter();
	}
}
