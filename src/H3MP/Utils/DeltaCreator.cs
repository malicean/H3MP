using System;

namespace H3MP.Utils
{
	public readonly struct DeltaCreator<T>
	{
		private readonly T _this;
		private readonly T _baseline;

		public DeltaCreator(T @this, T baseline)
		{
			_this = @this;
			_baseline = baseline;
		}

		public Option<TValue> Create<TValue>(Func<T, Option<TValue>> optionOf) where TValue : IDeltable<TValue, TValue>
		{
			return Create(optionOf, x => x);
		}

		public Option<TValue>[] Create<TValue>(Func<T, Option<TValue>[]> optionsOf) where TValue : IDeltable<TValue, TValue>
		{
			return Create(optionsOf, x => x);
		}

		public Option<TValue> Create<TValue, TDeltable>(Func<T, Option<TValue>> optionOf, Func<TValue, TDeltable> deltableOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			if (optionOf(_this).Map(deltableOf).MatchSome(out var thisDelta))
			{
				if (optionOf(_baseline).Map(deltableOf).MatchSome(out var baselineDelta))
				{
					return thisDelta.CreateDelta(baselineDelta);
				}

				return Option.Some(thisDelta.InitialDelta);
			}

			return Option.None<TValue>();
		}

		public Option<TValue>[] Create<TValue, TDeltable>(Func<T, Option<TValue>[]> optionsOf, Func<TValue, TDeltable> deltableOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			var thisOptions = optionsOf(_this);
			var baselineOptions = optionsOf(_baseline);
			var deltaOptions = new Option<TValue>[thisOptions.Length];

			for (var i = 0; i < deltaOptions.Length; ++i)
			{
				var thisOption = thisOptions[i];
				var baselineOption = baselineOptions[i];
				ref var delta = ref deltaOptions[i];

				if (thisOption.MatchSome(out var thisValue))
				{
					if (baselineOption.MatchSome(out var baselineValue))
					{
						delta = deltableOf(thisValue).CreateDelta(deltableOf(baselineValue));
					}
					else
					{
						delta = Option.Some(deltableOf(thisValue).InitialDelta);
					}
				}
				else
				{
					delta = Option.None<TValue>();
				}
			}

			return deltaOptions;
		}
	}
}
