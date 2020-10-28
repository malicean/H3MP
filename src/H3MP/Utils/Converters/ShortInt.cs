using System;

namespace H3MP.Utils
{
	public class ShortIntConverter : IConverter<short, int>, IConverter<int, short>
	{
		public int Convert(short value)
		{
			return value;
		}

		public short Convert(int value)
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
