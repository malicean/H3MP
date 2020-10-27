using System;

namespace H3MP.Utils
{
	public static class ArrayDeltaExtensions
	{
		public static ArrayDelta<TValue, TDelta, TDeltable> ToArray<TValue, TDelta, TDeltable>(this TDeltable @this) where TDeltable : IDeltable<TValue, TDelta>
		{
			return new ArrayDelta<TValue, TDelta, TDeltable>(@this);
		}
	}

	public readonly struct ArrayDelta<TValue, TDelta, TDeltable> : IDeltable<TValue[], Option<TDelta>[]> where TDeltable : IDeltable<TValue, TDelta>
	{
		private readonly TDeltable _deltable;

		public ArrayDelta(TDeltable deltable)
		{
			_deltable = deltable;
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

					var delta = _deltable.CreateDelta(now[i], baselineValue);
					deltas[i] = delta;
					dirty |= delta.IsSome;
				}
			}
			else
			{
				dirty = true;

				for (var i = 0; i < deltas.Length; ++i)
				{
					deltas[i] = _deltable.CreateDelta(now[i], Option.None<TValue>());
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

					values[i] = _deltable.ConsumeDelta(nowValue, baselineValue);
				}
			}
			else
			{
				for (var i = 0; i < now.Length; ++i)
				{
					values[i] = _deltable.ConsumeDelta(now[i].Unwrap(), Option.None<TValue>());
				}
			}

			return values;
		}
	}
}
