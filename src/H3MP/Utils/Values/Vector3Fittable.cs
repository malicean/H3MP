using System;
using UnityEngine;

namespace H3MP.Utils
{
	public static class Vector3FittableExtension
	{
		public static Vector3Fittable ToFittable(this Vector3 @this)
		{
			return new Vector3Fittable(@this);
		}
	}

    public readonly struct Vector3Fittable : ILinearFittable<Vector3Fittable>, IRef<Vector3>
	{
		private readonly Vector3 _value;

		public Vector3 InitialDelta => _value;

		public Vector3 Value => _value;

		public Vector3Fittable(Vector3 value)
        {
            _value = value;
        }

		public Vector3Fittable Fit(Vector3Fittable other, double t)
		{
			return Vector3.LerpUnclamped(_value, other._value, (float) t).ToFittable();
		}

		public static implicit operator Vector3(Vector3Fittable @this)
		{
			return @this._value;
		}
	}
}
