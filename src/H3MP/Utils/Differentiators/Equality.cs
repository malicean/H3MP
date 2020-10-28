using System;

namespace H3MP.Utils
{
	public class EqualityDifferentiator<TValue> : IDifferentiator<TValue, TValue> where TValue : IEquatable<TValue>
	{
		public static IDifferentiator<TValue, TValue> Instance { get; } = new EqualityDifferentiator<TValue>();

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

		public TValue ConsumeDelta(TValue delta, Option<TValue> now)
		{
			return delta;
		}
	}
}
