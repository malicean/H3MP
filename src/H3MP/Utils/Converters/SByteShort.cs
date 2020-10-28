using System;

namespace H3MP.Utils
{
	public class SByteShortConverter : IConverter<sbyte, short>, IConverter<short, sbyte>
	{
		public short Convert(sbyte value)
		{
			return value;
		}

		public sbyte Convert(short value)
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
