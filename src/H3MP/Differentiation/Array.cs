using System;
using H3MP.Utils;

namespace H3MP.Differentiation
{
	public static class ArrayDifferentiatorExtensions
	{
		public static IDifferentiator<TValue[], Option<TDelta>[]> ToArray<TValue, TDelta>(this IDifferentiator<TValue, TDelta> @this)
		{
			return new ArrayDifferentiator<TValue, TDelta>(@this);
		}
	}

	public class ArrayDifferentiator<TValue, TDelta> : IDifferentiator<TValue[], Option<TDelta>[]>
	{
		private readonly IDifferentiator<TValue, TDelta> _differentiator;

		public ArrayDifferentiator(IDifferentiator<TValue, TDelta> differentiator)
		{
			_differentiator = differentiator;
		}

		public TValue[] ConsumeDelta(Option<TDelta>[] delta, Option<TValue[]> now)
		{
			var values = new TValue[delta.Length];
			if (now.MatchSome(out var nowArray))
			{
				for (var i = 0; i < values.Length; ++i)
				{
					values[i] = delta[i].MatchSome(out var deltaValue)
						? _differentiator.ConsumeDelta(deltaValue, Option.Some(nowArray[i]))
						: nowArray[i];
				}
			}
			else
			{
				for (var i = 0; i < values.Length; ++i)
				{
					values[i] = delta[i].MatchSome(out var deltaValue)
						? _differentiator.ConsumeDelta(deltaValue, Option.None<TValue>())
						: default;
				}
			}

			return values;
		}

		public Option<Option<TDelta>[]> CreateDelta(TValue[] now, Option<TValue[]> baseline)
		{
			var deltas = new Option<TDelta>[now.Length];
			var dirty = false;
			if (baseline.MatchSome(out var baselineArray))
			{
				if (now.Length != baselineArray.Length)
				{
					throw new FormatException("Now array and baseline array must have mismatching lengths.");
				}

				for (var i = 0; i < deltas.Length; ++i)
				{
					var delta = _differentiator.CreateDelta(now[i], Option.Some(baselineArray[i]));
					deltas[i] = delta;
					dirty |= delta.IsSome;
				}
			}
			else
			{
				dirty = true;

				for (var i = 0; i < deltas.Length; ++i)
				{
					deltas[i] = _differentiator.CreateDelta(now[i], Option.None<TValue>());
				}
			}

			return dirty
				? Option.Some(deltas)
				: Option.None<Option<TDelta>[]>();
		}
	}
}
