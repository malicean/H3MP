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

    public readonly struct EqualityDelta<TValue> : IDeltable<EqualityDelta<TValue>, TValue>, IRef<TValue> where TValue : IEquatable<TValue>
	{
		private readonly TValue _value;

		public TValue InitialDelta => _value;

		public TValue Value => _value;

		public EqualityDelta(TValue value)
        {
            _value = value;
        }

		public Option<TValue> CreateDelta(EqualityDelta<TValue> baseline)
		{
			return _value.Equals(baseline._value) ? Option.None<TValue>() : Option.Some(_value);
		}

		public EqualityDelta<TValue> ConsumeDelta(TValue delta)
		{
			return _value.ToEqualityDelta();
		}

		public static implicit operator TValue(EqualityDelta<TValue> @this)
		{
			return @this._value;
		}
	}
}
