using System;

namespace H3MP.Utils
{
	public readonly struct ArrayEqualityDelta<TValue, TDelta, TDeltable> : IDeltable<TValue[], TDelta[]> where TDeltable : IDeltable<TDelta, TValue>
	{
		public Option<TDelta[]> CreateDelta(TValue[] now, Option<TValue[]> baseline)
		{
			var deltas = new TDelta[now.Length];
			if (baseline.MatchSome(out var value))
			{

			}

			return Option.Some(now);
		}

		public TValue[] ConsumeDelta(TDelta[] now, Option<TValue[]> baseline)
		{
			throw new NotImplementedException();
		}
	}
}
