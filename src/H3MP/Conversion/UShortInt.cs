using System;

namespace H3MP.Conversion
{
	public class UShortIntConverter : IConverter<ushort, int>, IConverter<int, ushort>
	{
		public int Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(int value)
		{
			const ushort min = ushort.MinValue;
			const ushort max = ushort.MaxValue;
			if (value < min || max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within the range of a ushort: " + min + " <= x <= " + max);
			}

			return (ushort) value;
		}
	}
}
