using System;

namespace H3MP.Conversion
{
	public class SByteIntConverter : IConverter<sbyte, int>, IConverter<int, sbyte>
	{
		public int Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(int value)
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
