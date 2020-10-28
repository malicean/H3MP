using UnityEngine;

namespace H3MP.Utils
{
	public class QuaternionFitter : IFitter<Quaternion>
	{
		public Quaternion Fit(Quaternion a, Quaternion b, float t)
		{
			return Quaternion.LerpUnclamped(a, b, t);
		}
	}
}
