using System.Runtime.InteropServices;
using H3MP.IO;

namespace H3MP.Serialization
{
	public readonly struct DoubleSerializer : ISerializer<double>
	{
		public double Deserialize(ref BitPackReader reader)
		{
			DoubleToULong conv = default;

			conv.Integral = reader.Bytes.Pop();
			conv.Integral |= (ulong) reader.Bytes.Pop() << 8;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 16;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 24;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 32;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 40;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 48;
			conv.Integral |= (ulong) reader.Bytes.Pop() << 56;

			return conv.Floating;
		}

		public void Serialize(ref BitPackWriter writer, double value)
		{
			DoubleToULong conv = new DoubleToULong
			{
				Floating = value
			};

			writer.Bytes.Push((byte) conv.Integral);
			writer.Bytes.Push((byte) (conv.Integral >> 8));
			writer.Bytes.Push((byte) (conv.Integral >> 16));
			writer.Bytes.Push((byte) (conv.Integral >> 24));
			writer.Bytes.Push((byte) (conv.Integral >> 32));
			writer.Bytes.Push((byte) (conv.Integral >> 40));
			writer.Bytes.Push((byte) (conv.Integral >> 48));
			writer.Bytes.Push((byte) (conv.Integral >> 56));
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleToULong
		{
			[FieldOffset(0)]
			public double Floating;

			[FieldOffset(0)]
			public ulong Integral;
		}
	}
}
