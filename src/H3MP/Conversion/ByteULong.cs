using System;

namespace H3MP.Conversion
{
	public class ByteULongConverter : IConverter<byte, ulong>, IConverter<ulong, byte>
	{
		public ulong Convert(byte value)
		{
			return value;
		}

		public byte Convert(ulong value)
		{
			const byte max = byte.MaxValue;
			if (max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be less than or equal to the max byte: " + max);
			}

			return (byte) value;
		}
	}
}
