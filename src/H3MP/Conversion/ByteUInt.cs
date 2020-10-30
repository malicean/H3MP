using System;

namespace H3MP.Conversion
{
	public class ByteUIntConverter : IConverter<byte, uint>, IConverter<uint, byte>
	{
		public uint Convert(byte value)
		{
			return value;
		}

		public byte Convert(uint value)
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
