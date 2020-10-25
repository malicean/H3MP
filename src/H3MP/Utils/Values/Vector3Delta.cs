using System;
using UnityEngine;

namespace H3MP.Utils
{
	public static class Vector3Extensions
	{
		public static Vector3Delta ToDelta(this Vector3 @this)
		{
			return new Vector3Delta(@this);
		}
	}

    public readonly struct Vector3Delta : IDeltable<Vector3Delta, Vector3>, IRef<Vector3>
	{
		private readonly Vector3 _value;

		public Vector3 InitialDelta => _value;

		Vector3 IRef<Vector3>.Value => _value;

		public Vector3Delta(Vector3 value)
        {
            _value = value;
        }

		public Option<Vector3> CreateDelta(Vector3Delta baseline)
		{
			var delta = _value - baseline._value;

			return delta == Vector3.zero
				? Option.None<Vector3>()
				: Option.Some(delta);
		}

		public Vector3Delta ConsumeDelta(Vector3 delta)
		{
			return delta.ToDelta();
		}

		public static implicit operator Vector3(Vector3Delta @this)
		{
			return @this._value;
		}
	}
}
