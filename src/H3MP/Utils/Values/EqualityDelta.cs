using System;

namespace H3MP.Utils
{
	public static class EqualityDeltaExtensions
	{
		public static EqualityDelta<T> ToEqualityDelta<T>(this T @this) where T : IEquatable<T>
		{
			return new EqualityDelta<T>(@this);
		}
	}

    public readonly struct EqualityDelta<T> : IDeltable<EqualityDelta<T>, T>, IRef<T> where T : IEquatable<T>
	{
		private readonly T _value;

		public T InitialDelta => _value;

		public T Value => _value;

		public EqualityDelta(T value)
        {
            _value = value;
        }

		public Option<T> CreateDelta(EqualityDelta<T> baseline)
		{
			return _value.Equals(baseline._value) ? Option.None<T>() : Option.Some(_value);
		}

		public EqualityDelta<T> ConsumeDelta(T delta)
		{
			return _value.ToEqualityDelta();
		}

		public static implicit operator T(EqualityDelta<T> @this)
		{
			return @this._value;
		}
	}
}
