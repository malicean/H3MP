using System;
using UnityEngine;

namespace H3MP.Utils
{
	public static class UnitySerializer
	{
		public static Vector3Serializer<FloatSerializer<UIntSerializer<ByteSerializer>>> Vector3 { get; } = default;
		public static QuaternionSerializer<FloatSerializer<UIntSerializer<ByteSerializer>>> Quaternion { get; } = default;
		public static SmallestThreeQuaternionSerializer<FloatSerializer<UIntSerializer<ByteSerializer>>> SmallestThreeQuaternion { get; } = default;
	}

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

	public readonly struct QuaternionSerializer<TFloatSerializer> : ISerializer<Quaternion> where TFloatSerializer : ISerializer<float>
	{
		private readonly TFloatSerializer _float;

		public QuaternionSerializer(TFloatSerializer @float)
		{
			_float = @float;
		}

		public Quaternion Deserialize(ref BitPackReader reader)
		{
			return new Quaternion(
				_float.Deserialize(ref reader),
				_float.Deserialize(ref reader),
				_float.Deserialize(ref reader),
				_float.Deserialize(ref reader)
			);
		}

		public void Serialize(ref BitPackWriter writer, Quaternion value)
		{
			_float.Serialize(ref writer, value.x);
			_float.Serialize(ref writer, value.y);
			_float.Serialize(ref writer, value.z);
			_float.Serialize(ref writer, value.w);
		}
	}

	public readonly struct SmallestThreeQuaternionSerializer<TFloatSerializer> : ISerializer<Quaternion> where TFloatSerializer : ISerializer<float>
	{
		private enum QuaternionComponent : byte
		{
			X = 0b00,
			Y = 0b01,
			Z = 0b10,
			W = 0b11
		}

		private static bool HasComponent(QuaternionComponent value, QuaternionComponent component)
		{
			return (value & component) == component;
		}

		private static QuaternionComponent LargestComponent(Quaternion rotation, out float value)
		{
			QuaternionComponent component = QuaternionComponent.X;
			value = rotation.x;

			for (var i = 1; i < 4; ++i)
			{
				var iValue = rotation[i];
				if (iValue > value)
				{
					component = (QuaternionComponent) i;
					value = iValue;
				}
			}

			return component;
		}

		private readonly TFloatSerializer _float;

		public SmallestThreeQuaternionSerializer(TFloatSerializer @float)
		{
			_float = @float;
		}

		public Quaternion Deserialize(ref BitPackReader reader)
		{
			QuaternionComponent largest = default;
			if (reader.Bits.Pop())
			{
				largest |= (QuaternionComponent) 0b10;
			}
			if (reader.Bits.Pop())
			{
				largest |= (QuaternionComponent) 0b01;
			}

			var a = PrimitiveSerializer.Float.Deserialize(ref reader);
			var b = PrimitiveSerializer.Float.Deserialize(ref reader);
			var c = PrimitiveSerializer.Float.Deserialize(ref reader);
			var largestValue = Mathf.Sqrt(1 - a * a  - b * b - c * c);

			float x;
			float y;
			float z;
			float w;
			switch (largest)
			{
				case QuaternionComponent.X:
					x = largestValue;
					y = a;
					z = b;
					w = c;
					break;

				case QuaternionComponent.Y:
					x = a;
					y = largestValue;
					z = b;
					w = c;
					break;

				case QuaternionComponent.Z:
					x = a;
					y = b;
					z = largestValue;
					w = c;
					break;

				case QuaternionComponent.W:
					x = a;
					y = b;
					z = c;
					w = largestValue;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			return new Quaternion(x, y, z, w);
		}

		public void Serialize(ref BitPackWriter writer, Quaternion value)
		{
			var largestComponent = LargestComponent(value, out var largestValue);
			if (largestValue < 0)
			{
				value = new Quaternion(-value.x, -value.y, -value.z, -value.w);
			}

			float a;
			float b;
			float c;
			switch (largestComponent)
			{
				case QuaternionComponent.X:
					a = value.y;
					b = value.z;
					c = value.w;
					break;

				case QuaternionComponent.Y:
					a = value.x;
					b = value.z;
					c = value.w;
					break;

				case QuaternionComponent.Z:
					a = value.x;
					b = value.y;
					c = value.w;
					break;

				case QuaternionComponent.W:
					a = value.x;
					b = value.y;
					c = value.z;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			PrimitiveSerializer.Bool.Serialize(ref writer, HasComponent(largestComponent, (QuaternionComponent) 0b10));
			PrimitiveSerializer.Bool.Serialize(ref writer, HasComponent(largestComponent, (QuaternionComponent) 0b01));
			PrimitiveSerializer.Float.Serialize(ref writer, a);
			PrimitiveSerializer.Float.Serialize(ref writer, b);
			PrimitiveSerializer.Float.Serialize(ref writer, c);
		}
	}
}
