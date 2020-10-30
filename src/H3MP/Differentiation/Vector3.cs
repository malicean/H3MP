using H3MP.Utils;
using UnityEngine;

namespace H3MP.Differentiation
{
	public class Vector3Differentiator : IDifferentiator<Vector3, Vector3>
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

		public Vector3 ConsumeDelta(Vector3 delta, Option<Vector3> now)
		{
			return now.MatchSome(out var baselineValue)
				? delta + baselineValue
				: delta;
		}
	}
}
