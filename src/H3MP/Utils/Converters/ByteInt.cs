using System;

namespace H3MP.Utils
{
	public class ByteIntConverter : IConverter<byte, int>, IConverter<int, byte>
	{
		public int Convert(byte value)
		{
			return value;
		}

		public byte Convert(int value)
		{
			const byte min = byte.MinValue;
			const byte max = byte.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of a byte: " + min + " <= x <= " + max);
			}

			return (byte) value;
		}
	}
}
