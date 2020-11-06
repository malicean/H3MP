using System;

namespace H3MP.Conversion
{
	public class UShortFloatConverter : IConverter<ushort, float>, IConverter<float, ushort>
	{
		private const ushort MAX = ushort.MaxValue;

		private readonly float _max;

		public UShortFloatConverter(float max)
		{
			_max = max;
		}

		public float Convert(ushort value)
		{
			return (float) value / MAX * _max;
		}

		public ushort Convert(float value)
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative.");
			}

			if (value > _max)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"Value must be less than or equal to the max value ({_max}).");
			}

			return (ushort) (value / _max * MAX);
		}
	}
}
