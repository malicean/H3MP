using System;

namespace H3MP.Utils
{
	public class UShortFloatConverter : IConverter<ushort, float>, IConverter<float, ushort>
	{
		private const ushort MAX = ushort.MaxValue;

		private readonly float _maxAbs;

		public UShortFloatConverter(float maxAbs)
		{
			_maxAbs = maxAbs;
		}

		public float Convert(ushort value)
		{
			return (float) value / MAX * _maxAbs;
		}

		public ushort Convert(float value)
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be non-negative.");
			}

			float conv = value / MAX;
			if (conv > MAX)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"Value after deflation must be less than or equal to the maximum: {MAX}. Was: {conv:N0}.");
			}

			return (ushort) conv;
		}
	}
}
