using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public readonly struct DeltaComparer<T>
	{
		private readonly T _this;
		private readonly T _head;

		public DeltaComparer(T @this, T head)
		{
			_this = @this;
			_head = head;
		}

		public Option<TValue> Create<TValue>(Func<T, Option<TValue>> optionOf) where TValue : IDeltable<TValue, TValue>
		{
			return Create(optionOf, x => x);
		}

		// Yes, these two functions could be compounded using Option.Map, but then the generic inference fails and thats worse to type out.
		public Option<TValue> Create<TValue, TDeltable>(Func<T, Option<TValue>> optionOf, Func<TValue, TDeltable> deltableOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			if (optionOf(_this).Map(deltableOf).MatchSome(out var thisDelta))
			{
				if (optionOf(_head).Map(deltableOf).MatchSome(out var headDelta))
				{
					return thisDelta.CreateDelta(headDelta);
				}

				return Option.Some(thisDelta.InitialDelta);
			}

			return Option.None<TValue>();
		}

		public Option<TValue> Consume<TValue>(Func<T, Option<TValue>> optionOf) where TValue : IDeltable<TValue, TValue>
		{
			return Consume<TValue, TValue>(optionOf, x => x, x => x);
		}

		public Option<TValue> Consume<TValue, TDeltable>(Func<T, Option<TValue>> optionOf, Func<TValue, TDeltable> deltableOf, Func<TDeltable, TValue> valueOf) where TDeltable : IDeltable<TDeltable, TValue>
		{
			var headOption = optionOf(_head);

			if (optionOf(_this).MatchSome(out var thisValue))
			{
				var thisDelta = deltableOf(thisValue);

				var value = headOption.MatchSome(out var headValue)
					? valueOf(thisDelta.ConsumeDelta(headValue))
					: thisValue; // initial

				return Option.Some(value);
			}

			return headOption;
		}
	}
}
