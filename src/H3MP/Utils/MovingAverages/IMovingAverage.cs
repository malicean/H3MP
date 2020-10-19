using System;

namespace H3MP.Utils
{
	public interface IMovingAverage
	{
		double Value { get; }

		void Push(double value);
	}
}
