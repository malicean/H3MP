using System;
using H3MP.Utils;

namespace H3MP.Differentiation
{
	public static class ArrayDifferentiatorExtensions
	{
		public static IDifferentiator<Option<TValue>[], Option<TDelta>[]> ToArray<TValue, TDelta>(this IDifferentiator<TValue, TDelta> @this)
		{
			return new ArrayDifferentiator<TValue, TDelta>(@this);
		}
	}

	public class ArrayDifferentiator<TValue, TDelta> : IDifferentiator<Option<TValue>[], Option<TDelta>[]>
	{
		private readonly IDifferentiator<TValue, TDelta> _differentiator;

		public ArrayDifferentiator(IDifferentiator<TValue, TDelta> differentiator)
		{
			_differentiator = differentiator;
		}

		public Option<TValue>[] ConsumeDelta(Option<TDelta>[] delta, Option<Option<TValue>[]> now)
		{
			var values = new Option<TValue>[delta.Length];
			if (now.MatchSome(out var nowArray))
			{
				for (var i = 0; i < delta.Length; ++i)
				{
					values[i] = delta[i].MatchSome(out var deltaValue)
						? Option.Some(_differentiator.ConsumeDelta(deltaValue, nowArray[i]))
						: nowArray[i];
				}
			}
			else
			{
				for (var i = 0; i < delta.Length; ++i)
				{
					values[i] = delta[i].MatchSome(out var deltaValue)
						? Option.Some(_differentiator.ConsumeDelta(deltaValue, Option.None<TValue>()))
						: Option.None<TValue>();
				}
			}

			return values;
		}

		public Option<Option<TDelta>[]> CreateDelta(Option<TValue>[] now, Option<Option<TValue>[]> baseline)
		{
			var deltas = new Option<TDelta>[now.Length];
			var dirty = false;
			if (baseline.MatchSome(out var baselineArray))
			{
				if (now.Length != baselineArray.Length)
				{
					throw new FormatException("Now array and baseline array have mismatching lengths.");
				}

				for (var i = 0; i < deltas.Length; ++i)
				{
					if (now[i].MatchSome(out var nowValue))
					{
						deltas[i] = _differentiator.CreateDelta(nowValue, baselineArray[i]);
						dirty = true;
					}
				}
			}
			else
			{
				for (var i = 0; i < deltas.Length; ++i)
				{
					if (now[i].MatchSome(out var nowValue))
					{
						deltas[i] = _differentiator.CreateDelta(nowValue, Option.None<TValue>());
						dirty = true;
					}
				}
			}

			return dirty
				? Option.Some(deltas)
				: Option.None<Option<TDelta>[]>();
		}
	}
}
