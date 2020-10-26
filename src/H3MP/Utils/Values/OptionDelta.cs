using System;
using UnityEngine;

namespace H3MP.Utils
{
	public static class OptionDeltaExtensions
	{
		public static OptionDelta<T> ToDelta<T>(this Option<T> @this) where T : IDeltable<T, T>
		{
			return new OptionDelta<T>(@this);
		}
	}

    public readonly struct OptionDelta<T> : IDeltable<OptionDelta<T>, Option<T>>, IRef<Option<T>> where T : IDeltable<T, T>
	{
		private readonly Option<T> _value;

		public Option<T> Value => _value;

		public Option<T> InitialDelta => _value.Map(x => x.InitialDelta);

		public OptionDelta(Option<T> value)
        {
            _value = value;
        }

		public Option<Option<T>> CreateDelta(OptionDelta<T> baseline)
		{
			if (_value.MatchSome(out var thisValue))
			{
				if (baseline._value.MatchSome(out var baselineValue))
				{
					return Option.Some(thisValue.CreateDelta(baselineValue));
				}

				return Option.Some(Option.Some(thisValue.InitialDelta));
			}

			return Option.None<Option<T>>();
		}

		public OptionDelta<T> ConsumeDelta(Option<T> delta)
		{
			if (_value.MatchSome(out var thisValue))
			{
				if (delta.MatchSome(out var deltaValue))
				{
					return Option.Some(thisValue.ConsumeDelta(deltaValue)).ToDelta();
				}

				return _value.ToDelta();
			}

			return delta.ToDelta();
		}

		public static implicit operator Option<T>(OptionDelta<T> @this)
		{
			return @this._value;
		}
	}
}
