using System;
using UnityEngine;

namespace H3MP.Utils
{
	public class Vector3Serializer : ISerializer<Vector3>
	{
		private readonly ISerializer<float> _float;

		public Vector3Serializer(ISerializer<float> @float)
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
