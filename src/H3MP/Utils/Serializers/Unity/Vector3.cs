using System;
using UnityEngine;

namespace H3MP.Utils
{
	public readonly struct Vector3Serializer<TFloatSerializer> : ISerializer<Vector3> where TFloatSerializer : ISerializer<float>
	{
		private readonly TFloatSerializer _float;

		public Vector3Serializer(TFloatSerializer @float)
		{
			_float = @float;
		}

		public Vector3 Deserialize(ref BitPackReader reader)
		{
			return new Vector3(
				_float.Deserialize(ref reader),
				_float.Deserialize(ref reader),
				_float.Deserialize(ref reader)
			);
		}

		public void Serialize(ref BitPackWriter writer, Vector3 value)
		{
			_float.Serialize(ref writer, value.x);
			_float.Serialize(ref writer, value.y);
			_float.Serialize(ref writer, value.z);
		}
	}
}
