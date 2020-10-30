using System;

namespace H3MP.Conversion
{
	public class UIntLongConverter : IConverter<uint, long>, IConverter<long, uint>
	{
		public long Convert(uint value)
		{
			return value;
		}

		public uint Convert(long value)
		{
			const uint min = uint.MinValue;
			const uint max = uint.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of a uint: " + min + " <= x <= " + max);
			}

			return (uint) value;
		}
	}
}
