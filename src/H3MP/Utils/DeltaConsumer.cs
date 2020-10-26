using System;

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

		public Option<TValue>[] Consume<TValue>(Func<T, Option<TValue>[]> optionOf) where TValue : IDeltable<TValue, TValue>
		{
			return Consume<TValue, TValue>(optionOf, x => x, x => x);
		}

		public Option<TValue> Consume<TValue, TDeltable>(Func<T, Option<TValue>> optionOf, Func<TValue, TDeltable> deltableOf, Func<TDeltable, TValue> valueOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			var deltaOption = optionOf(_delta);

			if (optionOf(_this).MatchSome(out var thisValue))
			{
				var thisDelta = deltableOf(thisValue);

				var value = deltaOption.MatchSome(out var baselineValue)
					? valueOf(thisDelta.ConsumeDelta(baselineValue))
					: thisValue; // initial

				return Option.Some(value);
			}

			return deltaOption;
		}

		public Option<TValue>[] Consume<TValue, TDeltable>(Func<T, Option<TValue>[]> optionsOf, Func<TValue, TDeltable> deltableOf, Func<TDeltable, TValue> valueOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			var thisOptions = optionsOf(_this);
			var deltaOptions = optionsOf(_delta);
			var consumedOptions = new Option<TValue>[thisOptions.Length];

			for (var i = 0; i < deltaOptions.Length; ++i)
			{
				var thisOption = thisOptions[i];
				var deltaOption = deltaOptions[i];
				ref var consumed = ref consumedOptions[i];

				if (thisOption.MatchSome(out var thisValue))
				{
					if (deltaOption.MatchSome(out var deltaValue))
					{
						consumed = Option.Some(valueOf(deltableOf(thisValue).ConsumeDelta(deltaValue)));
					}
					else
					{
						consumed = thisOption;
					}
				}
				else
				{
					consumed = deltaOption;
				}
			}

			return deltaOptions;
		}
	}
}
