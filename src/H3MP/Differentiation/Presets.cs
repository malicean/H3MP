using System;
using UnityEngine;

namespace H3MP.Differentiation
{
	public static class Differentiators
	{
		public static IDifferentiator<Vector3, Vector3> Vector3 { get; } = new Vector3Differentiator();
		public static IDifferentiator<Quaternion, Quaternion> Quaternion { get; } = new QuaternionDifferentiator();

		public static IDifferentiator<T, T> Equality<T>() where T : IEquatable<T>
		{
			return EqualityDifferentiator<T>.Instance;
		}
	}
}
