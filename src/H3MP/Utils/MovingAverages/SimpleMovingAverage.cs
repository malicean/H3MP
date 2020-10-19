using System;
using System.Collections.Generic;
using System.Linq;

namespace H3MP.Utils
{
	// https://en.wikipedia.org/wiki/Moving_average#Simple_moving_average
	public class SimpleMovingAverage : IMovingAverage
	{
        private readonly Queue<double> _values;
        private readonly int _size;

		public double Value { get; private set; }

		public SimpleMovingAverage(double initialValue, int size)
		{
            _values = new Queue<double>(size);
            _size = size;

			Push(initialValue);
		}

		public void Push(double value)
		{
            while (_values.Count >= _size)
            {
                _values.Dequeue();
            }

            _values.Enqueue(value);
            Value = _values.Average();
		}
	}
}
