using UnityEngine;

namespace H3MP.Utils
{
	public static class QuaternionDeltaExtension
	{
		public static QuaternionDelta ToDelta(this Quaternion @this)
		{
			return new QuaternionDelta(@this);
		}
	}

    public readonly struct QuaternionDelta : IDeltable<QuaternionDelta, Quaternion>, IRef<Quaternion>
	{
		private readonly Quaternion _value;

		public Quaternion InitialDelta => _value;

		Quaternion IRef<Quaternion>.Value => _value;

		public QuaternionDelta(Quaternion value)
        {
            _value = value;
        }

		public Option<Quaternion> CreateDelta(QuaternionDelta baseline)
		{
			var delta = _value * Quaternion.Inverse(baseline);

			return delta == Quaternion.identity
				? Option.None<Quaternion>()
				: Option.Some(delta);
		}

		public QuaternionDelta ConsumeDelta(Quaternion delta)
		{
			return (_value * delta).ToDelta();
		}

		public static implicit operator Quaternion(QuaternionDelta @this)
		{
			return @this._value;
		}
	}
}
