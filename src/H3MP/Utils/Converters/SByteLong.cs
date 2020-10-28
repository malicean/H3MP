using System;

namespace H3MP.Utils
{
	public class SByteLongConverter : IConverter<sbyte, long>, IConverter<long, sbyte>
	{
		public long Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(long value)
		{
			const sbyte min = sbyte.MinValue;
			const sbyte max = sbyte.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of an sbyte: " + min + " <= x <= " + max);
			}

			return (sbyte) value;
		}
	}
}
