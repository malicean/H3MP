using UnityEngine;

namespace H3MP.Utils
{
	public class Vector3Fitter : IFitter<Vector3>
	{
		public Vector3 Fit(Vector3 a, Vector3 b, float t)
		{
			return Vector3.Lerp(a, b, t);
		}
	}
}
