using System;

namespace H3MP.Utils
{
	public readonly struct EqualityDelta<TValue> : IDeltable<TValue, TValue> where TValue : IEquatable<TValue>
	{
		public Option<TValue> CreateDelta(TValue now, Option<TValue> baseline)
		{
			if (baseline.MatchSome(out var value))
			{
				return now.Equals(value)
					? Option.None<TValue>()
					: Option.Some(now);
			}

			return Option.Some(now);
		}

		public TValue ConsumeDelta(TValue now, Option<TValue> baseline)
		{
			return now;
		}
	}
}
