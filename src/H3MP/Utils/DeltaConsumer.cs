using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public readonly struct DeltaConsumer<T>
	{
		private readonly T _this;
		private readonly T _delta;

		public DeltaConsumer(T @this, T delta)
		{
			_this = @this;
			_delta = delta;
		}

		public Option<TValue> Consume<TValue>(Func<T, Option<TValue>> optionOf) where TValue : IDeltable<TValue, TValue>
		{
			return Consume<TValue, TValue>(optionOf, x => x, x => x);
		}

		public Option<TValue> Consume<TValue, TDeltable>(Func<T, Option<TValue>> optionOf, Func<TValue, TDeltable> deltableOf, Func<TDeltable, TValue> valueOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			var baselineOption = optionOf(_delta);

			if (optionOf(_this).MatchSome(out var thisValue))
			{
				var thisDelta = deltableOf(thisValue);

				var value = baselineOption.MatchSome(out var baselineValue)
					? valueOf(thisDelta.ConsumeDelta(baselineValue))
					: thisValue; // initial

				return Option.Some(value);
			}

			return baselineOption;
		}
	}
}
