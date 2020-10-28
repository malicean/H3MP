using System;

namespace H3MP.Utils
{
	public class IntLongConverter : IConverter<int, long>, IConverter<long, int>
	{
		public long Convert(int value)
		{
			return value;
		}

		public int Convert(long value)
		{
			const int min = int.MinValue;
			const int max = int.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of an int: " + min + " <= x <= " + min);
			}

			return (int) value;
		}
	}
}
