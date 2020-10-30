using System;
using H3MP.Utils;

namespace H3MP.Differentiation
{
	public struct SuperDeltaConsumer<TDelta, TValue> where TValue : new()
	{
		private readonly TDelta _delta;
		private readonly Option<TValue> _now;

		private TValue _body;
		public TValue Body => _body;

		public SuperDeltaConsumer(TDelta delta, Option<TValue> now)
		{
			_delta = delta;
			_now = now;

			_body = new TValue();
		}

		public void Include<TValueChild, TDeltaChild>(Func<TDelta, Option<TDeltaChild>> deltaGetter, Func<TValue, TValueChild> valueGetter, ChildSetter<TValue, TValueChild> valueSetter, IDifferentiator<TValueChild, TDeltaChild> differentiator)
		{
			var delta = deltaGetter(_delta);

			if (delta.MatchSome(out var deltaValue))
			{
				valueSetter(ref _body, differentiator.ConsumeDelta(deltaValue, _now.Map(valueGetter)));
			}
		}
	}
}
