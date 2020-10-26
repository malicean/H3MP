using UnityEngine;

namespace H3MP.Utils
{
	public static class QuaternionFittableExtension
	{
		public static QuaternionFittable ToFittable(this Quaternion @this)
		{
			return new QuaternionFittable(@this);
		}
	}

    public readonly struct QuaternionFittable : ILinearFittable<QuaternionFittable>, IRef<Quaternion>
	{
		private readonly Quaternion _value;

		public Quaternion InitialDelta => _value;

		public Quaternion Value => _value;

		public QuaternionFittable(Quaternion value)
        {
            _value = value;
        }

		public QuaternionFittable Fit(QuaternionFittable other, double t)
		{
			return Quaternion.LerpUnclamped(_value, other._value, (float) t).ToFittable();
		}

		public static implicit operator Quaternion(QuaternionFittable @this)
		{
			return @this._value;
		}
	}
}
