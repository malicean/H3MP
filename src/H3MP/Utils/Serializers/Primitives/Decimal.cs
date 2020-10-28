using System.Runtime.InteropServices;

namespace H3MP.Utils
{
	public readonly struct DecimalSerializer : ISerializer<decimal>
	{
		public decimal Deserialize(ref BitPackReader reader)
		{
			DecimalToULongs conv = default;

			conv.Integral1 = reader.Bytes.Pop();
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 8;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 16;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 24;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 32;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 40;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 48;
			conv.Integral1 |= (ulong) reader.Bytes.Pop() << 56;

			conv.Integral2 = reader.Bytes.Pop();
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 8;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 16;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 24;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 32;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 40;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 48;
			conv.Integral2 |= (ulong) reader.Bytes.Pop() << 56;

			return conv.Floating;
		}

		public void Serialize(ref BitPackWriter writer, decimal value)
		{
			var conv = new DecimalToULongs
			{
				Floating = value
			};

			writer.Bytes.Push((byte) conv.Integral1);
			writer.Bytes.Push((byte) (conv.Integral1 >> 8));
			writer.Bytes.Push((byte) (conv.Integral1 >> 16));
			writer.Bytes.Push((byte) (conv.Integral1 >> 24));
			writer.Bytes.Push((byte) (conv.Integral1 >> 32));
			writer.Bytes.Push((byte) (conv.Integral1 >> 40));
			writer.Bytes.Push((byte) (conv.Integral1 >> 48));
			writer.Bytes.Push((byte) (conv.Integral1 >> 56));

			writer.Bytes.Push((byte) conv.Integral2);
			writer.Bytes.Push((byte) (conv.Integral2 >> 8));
			writer.Bytes.Push((byte) (conv.Integral2 >> 16));
			writer.Bytes.Push((byte) (conv.Integral2 >> 24));
			writer.Bytes.Push((byte) (conv.Integral2 >> 32));
			writer.Bytes.Push((byte) (conv.Integral2 >> 40));
			writer.Bytes.Push((byte) (conv.Integral2 >> 48));
			writer.Bytes.Push((byte) (conv.Integral2 >> 56));
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalToULongs
		{
			[FieldOffset(0)]
			public decimal Floating;

			[FieldOffset(0)]
			public ulong Integral1;

			[FieldOffset(sizeof(ulong))]
			public ulong Integral2;
		}
	}
}
