using H3MP.IO;
using UnityEngine;

namespace H3MP.Serialization
{
	public static class Vector3SerializerExtensions
	{
		public static ISerializer<Vector3> ToVector3(this ISerializer<float> @this)
		{
			return new Vector3Serializer(@this);
		}
	}

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
