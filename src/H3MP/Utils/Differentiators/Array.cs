namespace H3MP.Utils
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

		public Option<Option<TDelta>[]> CreateDelta(TValue[] now, Option<TValue[]> baseline)
		{
			var deltas = new Option<TDelta>[now.Length];
			var dirty = false;
			if (baseline.MatchSome(out var baselineArray))
			{
				for (var i = 0; i < deltas.Length; ++i)
				{
					var baselineValue = i < baselineArray.Length
						? Option.Some(baselineArray[i])
						: Option.None<TValue>();

					var delta = _differentiator.CreateDelta(now[i], baselineValue);
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

		public TValue[] ConsumeDelta(Option<TDelta>[] now, Option<TValue[]> baseline)
		{
			var values = new TValue[now.Length];
			if (baseline.MatchSome(out var baselineArray))
			{
				for (var i = 0; i < now.Length; ++i)
				{
					var baselineValue = now[i].MatchSome(out var nowValue)
						? Option.Some(baselineArray[i])
						: Option.None<TValue>();

					values[i] = _differentiator.ConsumeDelta(nowValue, baselineValue);
				}
			}
			else
			{
				for (var i = 0; i < now.Length; ++i)
				{
					values[i] = _differentiator.ConsumeDelta(now[i].Unwrap(), Option.None<TValue>());
				}
			}

			return values;
		}
	}
}
