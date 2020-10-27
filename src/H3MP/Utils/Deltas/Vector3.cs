using UnityEngine;

namespace H3MP.Utils
{
	public readonly struct Vector3Delta : IDeltable<Vector3, Vector3>
	{
		public Option<Vector3> CreateDelta(Vector3 now, Option<Vector3> baseline)
		{
			var delta = baseline.MatchSome(out var value)
				? now - value
				: now;

			return delta == Vector3.zero
				? Option.None<Vector3>()
				: Option.Some(delta);
		}

		public Vector3 ConsumeDelta(Vector3 now, Option<Vector3> baseline)
		{
			return baseline.MatchSome(out var baselineValue)
				? now + baselineValue
				: now;
		}
	}
}
