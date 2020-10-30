using System;
using H3MP.Utils;

namespace H3MP.Differentiation
{
	public class EqualityDifferentiator<T> : IDifferentiator<T, T> where T : IEquatable<T>
	{
		public static EqualityDifferentiator<T> Instance { get; } = new EqualityDifferentiator<T>();

		public Option<T> CreateDelta(T now, Option<T> baseline)
		{
			if (baseline.MatchSome(out var value))
			{
				return now.Equals(value)
					? Option.None<T>()
					: Option.Some(now);
			}

			return Option.Some(now);
		}

		public T ConsumeDelta(T delta, Option<T> now)
		{
			return delta;
		}
	}
}
