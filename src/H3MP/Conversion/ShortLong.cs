using System;

namespace H3MP.Conversion
{
	public class ShortLongConverter : IConverter<short, long>, IConverter<long, short>
	{
		public long Convert(short value)
		{
			return value;
		}

		public short Convert(long value)
		{
			const short min = short.MinValue;
			const short max = short.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of a short: " + min + " <= x <= " + max);
			}

			return (short) value;
		}
	}
}
