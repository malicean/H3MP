using System;

namespace H3MP.Utils
{
	public class UShortULongConverter : IConverter<ushort, ulong>, IConverter<ulong, ushort>
	{
		public ulong Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(ulong value)
		{
			const ushort max = ushort.MaxValue;
			if (max < value)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be less than or equal to the max ushort: " + max);
			}

			return (ushort) value;
		}
	}
}
