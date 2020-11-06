using System;

namespace H3MP.Conversion
{
	public class ShortFloatConverter : IConverter<short, float>, IConverter<float, short>
	{
		private const short MAX = short.MaxValue;
		private const short MIN = short.MinValue;

		private readonly float _min;
		private readonly float _max;

		public ShortFloatConverter(float min, float max)
		{
			_min = min;
			_max = max;
		}

		public float Convert(short value)
		{
			short conversion;
			float deflation;

			if (value < 0)
			{
				conversion = MIN;
				deflation = _min;
			}
			else
			{
				conversion = MAX;
				deflation = _max;
			}

			return (float) value / conversion * deflation;
		}

		public short Convert(float value)
		{
			float deflation;
			short conversion;

			if (value < 0)
			{
				if (value < _min)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, $"Value must be greater than or equal to the min value ({_min}).");
				}

				deflation = _min;
				conversion = MIN;
			}
			else
			{
				if (value > _max)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, $"Value must be less than or equal to the max value ({_max}).");
				}

				deflation = _max;
				conversion = MAX;
			}
			
			return (short) (value / deflation * conversion);
		}
	}
}
