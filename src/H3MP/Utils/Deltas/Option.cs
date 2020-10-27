using System;
using UnityEngine;

namespace H3MP.Utils
{
	public static class OptionDeltaExtensions
	{
		public static OptionDelta<TValue, TDelta, TDeltable> ToOption<TValue, TDelta, TDeltable>(this TDeltable @this) where TDeltable : IDeltable<TValue, TDelta>
		{
			return new OptionDelta<TValue, TDelta, TDeltable>(@this);
		}
	}

	public readonly struct OptionDelta<TValue, TDelta, TDeltable> : IDeltable<Option<TValue>, Option<TDelta>> where TDeltable : IDeltable<TValue, TDelta>
	{
		private readonly TDeltable _deltable;

		public OptionDelta(TDeltable deltable)
		{
			_deltable = deltable;
		}

		public Option<Option<TDelta>> CreateDelta(Option<TValue> now, Option<Option<TValue>> baseline)
		{
			if (baseline.MatchSome(out var baselineValue))
			{
				if (now.MatchSome(out var nowValue))
				{
					// some -> some
					return Option.Some(_deltable.CreateDelta(nowValue, baselineValue));
				}

				// some -> none
				return Option.Some(Option.None<TDelta>());
			}
			else if (now.MatchSome(out var nowValue))
			{
				// none -> some
				return Option.Some(_deltable.CreateDelta(nowValue, Option.None<TValue>()));
			}

			// none -> none
			return Option.None<Option<TDelta>>();
		}

		public Option<TValue> ConsumeDelta(Option<TDelta> now, Option<Option<TValue>> baseline)
		{
			if (baseline.MatchSome(out var baselineValue))
			{
				if (now.MatchSome(out var nowValue))
				{
					// some -> some
					return Option.Some(_deltable.ConsumeDelta(nowValue, baselineValue));
				}

				// some -> none
				return Option.None<TValue>();
			}
			else if (now.MatchSome(out var nowValue))
			{
				// none -> some
				return Option.Some(_deltable.ConsumeDelta(nowValue, Option.None<TValue>()));
			}

			// none -> none
			return Option.None<TValue>();
		}
	}
}
