using System.Runtime.InteropServices;
using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct FloatSerializer : ISerializer<float>
	{
		public float Deserialize(ref BitPackReader reader)
		{
			FloatToUInt conv = default;

			conv.Integral = reader.Bytes.Pop();
			conv.Integral |= (uint) reader.Bytes.Pop() << 8;
			conv.Integral |= (uint) reader.Bytes.Pop() << 16;
			conv.Integral |= (uint) reader.Bytes.Pop() << 24;

			return conv.Floating;
		}

		public void Serialize(ref BitPackWriter writer, float value)
		{
			FloatToUInt conv = new FloatToUInt
			{
				Floating = value
			};

			writer.Bytes.Push((byte) conv.Integral);
			writer.Bytes.Push((byte) (conv.Integral >> 8));
			writer.Bytes.Push((byte) (conv.Integral >> 16));
			writer.Bytes.Push((byte) (conv.Integral >> 24));
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatToUInt
		{
			[FieldOffset(0)]
			public float Floating;

			[FieldOffset(0)]
			public uint Integral;
		}
	}
}
