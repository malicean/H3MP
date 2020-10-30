using H3MP.Utils;
using UnityEngine;

namespace H3MP.Differentiation
{
	public class QuaternionDifferentiator : IDifferentiator<Quaternion, Quaternion>
	{
		public Option<Quaternion> CreateDelta(Quaternion now, Option<Quaternion> baseline)
		{
			var delta = baseline.MatchSome(out var value)
				? now * Quaternion.Inverse(value)
				: now;

			return delta == Quaternion.identity
				? Option.None<Quaternion>()
				: Option.Some(delta);
		}

		public Quaternion ConsumeDelta(Quaternion delta, Option<Quaternion> now)
		{
			return now.MatchSome(out var baselineValue)
				? delta * baselineValue
				: delta;
		}
	}
}
