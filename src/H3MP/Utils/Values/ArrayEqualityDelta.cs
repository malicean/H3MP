using System;

namespace H3MP.Utils
{
	public static class ArrayEqualityDeltaExtensions
	{
		public static ArrayEqualityDelta<T> ToEqualityDelta<T>(this T[] @this) where T : IEquatable<T>
		{
			return new ArrayEqualityDelta<T>(@this);
		}
	}

    public readonly struct ArrayEqualityDelta<TValue> : IDeltable<ArrayEqualityDelta<TValue>, TValue[]>, IRef<TValue[]> where TValue : IEquatable<TValue>
	{
		private readonly TValue[] _value;

		public TValue[] InitialDelta => _value;

		public TValue[] Value => _value;

		public ArrayEqualityDelta(TValue[] value)
        {
            _value = value;
        }

		public Option<TValue[]> CreateDelta(ArrayEqualityDelta<TValue> baseline)
		{
			if (_value.Length == baseline._value.Length)
			{
				return Option.Some(_value);
			}

			for (var i = 0; i < _value.Length; ++i)
			{
				if (!_value[i].Equals(baseline._value))
				{
					return Option.Some(_value);
				}
			}

			return Option.None<TValue[]>();
		}

		public ArrayEqualityDelta<TValue> ConsumeDelta(TValue[] delta)
		{
			return _value.ToEqualityDelta();
		}

		public static implicit operator TValue[](ArrayEqualityDelta<TValue> @this)
		{
			return @this._value;
		}
	}
}
