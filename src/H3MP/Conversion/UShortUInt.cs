using System;

namespace H3MP.Conversion
{
	public class UShortUIntConverter : IConverter<ushort, uint>, IConverter<uint, ushort>
	{
		public uint Convert(ushort value)
		{
			return value;
		}

		public ushort Convert(uint value)
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
