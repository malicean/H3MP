using System;

namespace H3MP.Common.Utils
{
	// https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average
	// https://stackoverflow.com/a/44073605 with slight reformatting
	public class ExponentialMovingAverage
	{
		private readonly double _alpha;

		public double Value { get; private set; }

		public ExponentialMovingAverage(double initialValue, double alpha)
		{
			if (alpha <= 0 || 1 <= alpha)
			{
				throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "The accepted range of alpha is (0, 1).");
			}

			_alpha = alpha;

			Value = initialValue;
		}

		public void Push(double value)
		{
			// Simplified version of the Wikipedia formula:
			// Sₜ =	{ Yₜ,					t = 1
			//		{ αYₜ + (1 - α) * Sₜ₋₁,	t > 1
			//
			// Form. Name	Code Name		Range	Description
			// Sₜ			_lastAverage	(-∞, ∞)	Newly calculated EMA.
			// α			_alpha			( 0, 1)	Weight of newer values.
			// Yₜ			value			(-∞, ∞)	Input value.
			// Sₜ₋₁			_lastAverage	(-∞, ∞)	Last calculated EMA.
			//
			Value = (value - Value) * _alpha + Value;
		}
	}
}
