using H3MP.Utils;

namespace H3MP.Differentiation
{
	public static class OptionDifferentiatorExtensions
	{
		public static IDifferentiator<Option<TValue>, Option<TDelta>> ToOption<TValue, TDelta>(this IDifferentiator<TValue, TDelta> @this)
		{
			return new OptionDifferentiator<TValue, TDelta>(@this);
		}
	}

	public class OptionDifferentiator<TValue, TDelta> : IDifferentiator<Option<TValue>, Option<TDelta>>
	{
		private readonly IDifferentiator<TValue, TDelta> _differentiator;

		public OptionDifferentiator(IDifferentiator<TValue, TDelta> differentiator)
		{
			_differentiator = differentiator;
		}

		public Option<TValue> ConsumeDelta(Option<TDelta> delta, Option<Option<TValue>> now)
		{
			TDelta deltaValue;

			if (now.MatchSome(out var nowValue))
			{
				if (delta.MatchSome(out deltaValue))
				{
					// some -> some (different, same is filtered out before method call)
					return Option.Some(_differentiator.ConsumeDelta(deltaValue, nowValue));
				}

				// some -> none
				return Option.None<TValue>();
			}

			if (delta.MatchSome(out deltaValue))
			{
				// none -> some
				return Option.Some(_differentiator.ConsumeDelta(deltaValue, Option.None<TValue>()));
			}

			// none -> none
			return Option.None<TValue>();
		}

		public Option<Option<TDelta>> CreateDelta(Option<TValue> now, Option<Option<TValue>> baseline)
		{
			TValue nowValue;
			if (baseline.MatchSome(out var baselineValue))
			{
				if (now.MatchSome(out nowValue))
				{
					var delta = _differentiator.CreateDelta(nowValue, baselineValue);

					// some -> some
					return delta.IsSome
						? Option.Some(delta) // change
						: Option.None<Option<TDelta>>(); // no change
				}

				// some -> none
				return Option.Some(Option.None<TDelta>());
			}

			if (now.MatchSome(out nowValue))
			{
				// none -> some
				return Option.Some(_differentiator.CreateDelta(nowValue, Option.None<TValue>()));
			}

			// none -> none (no change)
			return Option.None<Option<TDelta>>();
		}
	}
}
