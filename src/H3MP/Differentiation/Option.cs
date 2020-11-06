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
			if (now.MatchSome(out var nowValue))
			{
				if (delta.MatchSome(out var deltaValue))
				{
					// some -> some
					return Option.Some(_differentiator.ConsumeDelta(deltaValue, nowValue));
				}

				// some -> none
				return Option.None<TValue>();
			}
			else if (delta.MatchSome(out var deltaValue))
			{
				// none -> some
				return Option.Some(_differentiator.ConsumeDelta(deltaValue, Option.None<TValue>()));
			}

			// none -> none
			return Option.None<TValue>();
		}

		public Option<Option<TDelta>> CreateDelta(Option<TValue> now, Option<Option<TValue>> baseline)
		{
			if (baseline.MatchSome(out var baselineValue))
			{
				if (now.MatchSome(out var nowValue))
				{
					// some -> some
					return Option.Some(_differentiator.CreateDelta(nowValue, baselineValue));
				}

				// some -> none
				return Option.Some(Option.None<TDelta>());
			}
			else if (now.MatchSome(out var nowValue))
			{
				// none -> some
				return Option.Some(_differentiator.CreateDelta(nowValue, Option.None<TValue>()));
			}

			// none -> none
			return Option.None<Option<TDelta>>();
		}
	}
}
