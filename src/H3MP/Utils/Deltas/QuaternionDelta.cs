using UnityEngine;

namespace H3MP.Utils
{
	public readonly struct QuaternionDelta : IDeltable<Quaternion, Quaternion>
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

		public Quaternion ConsumeDelta(Quaternion now, Option<Quaternion> baseline)
		{
			return baseline.MatchSome(out var baselineValue)
				? now * baselineValue
				: now;
		}
	}
}
