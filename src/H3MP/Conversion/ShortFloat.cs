using System;

namespace H3MP.Conversion
{
	public class ShortFloatConverter : IConverter<short, float>, IConverter<float, short>
	{
		private const short MAX = short.MaxValue;
		private const short MIN = short.MinValue;

		private readonly float _maxAbs;

		public ShortFloatConverter(float maxAbs)
		{
			_maxAbs = maxAbs;
		}

		public float Convert(short value)
		{
			return value < 0
				? (float) value / MIN * _maxAbs
				: (float) value / MAX * _maxAbs;
		}

		public short Convert(float value)
		{
			float conv = value / _maxAbs;
			if (conv < 0)
			{
				if (conv < MIN)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, $"Value after deflation must be greater than or equal to the minimum: {MIN}. Was: {conv:N0}.");
				}
			}
			else if (conv > MAX)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"Value after deflation must be less than or equal to the maximum: {MAX}. Was: {conv:N0}.");
			}

			return (short) conv;
		}
	}
}
