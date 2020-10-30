using System;

namespace H3MP.Conversion
{
	public class UIntULongConverter : IConverter<uint, ulong>, IConverter<ulong, uint>
	{
		public ulong Convert(uint value)
		{
			return value;
		}

		public uint Convert(ulong value)
		{
			const byte max = byte.MaxValue;
			if (max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be less than or equal to the max byte: " + max);
			}

			return (uint) value;
		}
	}
}
