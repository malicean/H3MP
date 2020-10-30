using System;
using H3MP.Utils;

namespace H3MP.Differentiation
{
	public struct SuperDeltaCreator<TValue, TDelta> where TDelta : IOptionComposite, new()
	{
		private readonly TValue _now;
		private readonly Option<TValue> _baseline;

		private TDelta _body;
		public Option<TDelta> Body => _body.HasSome ? Option.Some(_body) : Option.None<TDelta>();

		public SuperDeltaCreator(TValue now, Option<TValue> baseline)
		{
			_now = now;
			_baseline = baseline;
			_body = new TDelta();
		}

		public void Include<TValueChild, TDeltaChild>(Func<TValue, TValueChild> valueGetter, ChildSetter<TDelta, Option<TDeltaChild>> deltaSetter, IDifferentiator<TValueChild, TDeltaChild> differentiator)
		{
			var childValue = differentiator.CreateDelta(valueGetter(_now), _baseline.Map(valueGetter));

			deltaSetter(ref _body, childValue);
		}
	}
}
