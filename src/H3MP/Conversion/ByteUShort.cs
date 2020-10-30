using System;

namespace H3MP.Conversion
{
	public class ByteUShortConverter : IConverter<byte, ushort>, IConverter<ushort, byte>
	{
		public ushort Convert(byte value)
		{
			return value;
		}

		public byte Convert(ushort value)
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
