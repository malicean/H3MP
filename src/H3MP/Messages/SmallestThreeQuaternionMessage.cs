using System;
using H3MP.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Messages
{
	public struct SmallestThreeQuaternionMessage : IPackedSerializable, IRef<Quaternion>
	{
		private enum QuaternionComponent : byte
		{
			X = 0b00,
			Y = 0b01,
			Z = 0b10,
			W = 0b11
		}

		private static QuaternionComponent ComputeSmallestThree(Quaternion value, out float a, out float b, out float c)
		{
			QuaternionComponent largestComponent = QuaternionComponent.X;
			float largestValue = value.x;

			for (var i = 1; i < 4; ++i)
			{
				if (value[i] > largestValue)
                {
					largestComponent = (QuaternionComponent) i;
					largestValue = value[i];
                }
			}

			if (largestValue < 0)
			{
				largestValue = -largestValue;
				value = new Quaternion(-value.x, -value.y, -value.z, -value.w);
			}

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

			return largestComponent;
		}

		private QuaternionComponent _largest;
		private float _a;
		private float _b;
		private float _c;

		public Quaternion Value { get; private set; }

		public SmallestThreeQuaternionMessage(Quaternion value)
		{
			_largest = ComputeSmallestThree(value, out _a, out _b, out _c);

			Value = value;
		}

		public void Deserialize(ref BitPackReader reader)
		{
			_largest = default;
			if (reader.Bits.Dequeue())
			{
				_largest |= (QuaternionComponent) 0b10;
			}
			if (reader.Bits.Dequeue())
			{
				_largest |= (QuaternionComponent) 0b01;
			}

			_a = reader.Bytes.GetFloat();
			_b = reader.Bytes.GetFloat();
			_c = reader.Bytes.GetFloat();

			float largestValue = Mathf.Sqrt(1 - _a * _a  - _b * _b - _c * _c);

			float x;
			float y;
			float z;
			float w;
			switch (_largest)
			{
				case QuaternionComponent.X:
					x = largestValue;
					y = _a;
					z = _b;
					w = _c;
					break;

				case QuaternionComponent.Y:
					x = _a;
					y = largestValue;
					z = _b;
					w = _c;
					break;

				case QuaternionComponent.Z:
					x = _a;
					y = _b;
					z = largestValue;
					w = _c;
					break;

				case QuaternionComponent.W:
					x = _a;
					y = _b;
					z = _c;
					w = largestValue;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			Value = new Quaternion(x, y, z, w);
		}

		public void Serialize(ref BitPackWriter writer)
		{
			var largest = (byte) _largest;

			writer.Bits.Push(largest.HasFlag(0b10));
			writer.Bits.Push(largest.HasFlag(0b01));
			writer.Bytes.Put(_a);
			writer.Bytes.Put(_b);
			writer.Bytes.Put(_c);
		}
	}
}
