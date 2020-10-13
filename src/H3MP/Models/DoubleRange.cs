using System;

namespace H3MP.Models
{
	public readonly struct DoubleRange
	{
		public static DoubleRange AllNumbers { get; } = new DoubleRange(double.NegativeInfinity, double.PositiveInfinity);

		public double Minimum { get; }

		public double Maximum { get; }

		public DoubleRange(double minimum, double maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public bool IsBetween(double value)
		{
			return Minimum < value && value < Maximum;
		}

		public bool IsWithin(double value)
		{
			return Minimum <= value && value <= Maximum;
		}

		public double Clamp(double value)
		{
			return Math.Max(Minimum, Math.Min(Maximum, value));
		}

		public DoubleRange Clamp(DoubleRange inner)
		{
			return new DoubleRange(
				Math.Max(Minimum, inner.Minimum),
				Math.Min(Maximum, inner.Maximum)
			);
		}
	}
}
