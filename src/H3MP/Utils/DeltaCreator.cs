using System;
using LiteNetLib.Utils;
using UnityEngine;

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
	}
}
