using UnityEngine;

namespace H3MP.Utils
{
	internal static class Fitting
	{
		public static double Fit(this double a, double b, double t)
		{
			return a + (b - a) * t;
		}

		public static Vector3 Fit(this Vector3 a, Vector3 b, double t)
		{
			return Vector3.LerpUnclamped(a, b, (float) t);
		}

		public static Quaternion Fit(this Quaternion a, Quaternion b, double t)
		{
			return Quaternion.LerpUnclamped(a, b, (float) t);
		}
	}
}