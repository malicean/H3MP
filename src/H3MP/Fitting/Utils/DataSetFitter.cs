using System;
using System.Collections.Generic;

namespace H3MP.Fitting
{
	public class DataSetFitter<TTime, TValue>
	{
		private readonly IComparer<TTime> _timeComparer;
		private readonly IInverseFitter<TTime> _timeInverseFitter;
		private readonly IFitter<TValue> _valueFitter;

		public DataSetFitter(IComparer<TTime> timeComparer, IInverseFitter<TTime> timeInverseFitter, IFitter<TValue> valueFitter)
		{
			_timeComparer = timeComparer;
			_timeInverseFitter = timeInverseFitter;
			_valueFitter = valueFitter;
		}

		private TValue Fit(KeyValuePair<TTime, TValue> a, KeyValuePair<TTime, TValue> b, TTime time)
		{
			var t = _timeInverseFitter.InverseFit(a.Key, b.Key, time);
			return _valueFitter.Fit(a.Value, b.Value, t);
		}

		private TValue FitAsHead(List<KeyValuePair<TTime, TValue>> dataSet, int head, TTime time)
		{
			var first = dataSet[head - 1];
			var second = dataSet[head];

			return Fit(first, second, time);
		}

		public TValue Fit(List<KeyValuePair<TTime, TValue>> dataSet, TTime time)
		{
			if (dataSet.Count == 0)
			{
				throw new InvalidOperationException("No data exists to fit.");
			}

			var i = dataSet.Count - 1;
			var last = dataSet[i];

			// only 1 data point
			if (i == 0)
			{
				return last.Value;
			}

			// [last, infinity]
			switch (_timeComparer.Compare(time, last.Key))
			{
				case -1:
					break;
				case 0:
					return last.Value;
				case 1:
					return FitAsHead(dataSet, i, time);

				default:
					throw new ArgumentOutOfRangeException();
			}

			// (first, last)
			for (; i > 0; --i)
			{
				var a = dataSet[i - 1];
				var b = dataSet[i];

				switch (_timeComparer.Compare(time, a.Key))
				{
					case -1:
						continue;
					case 0:
						return a.Value;
					case 1:
						// ahead
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (_timeComparer.Compare(time, b.Key))
				{
					case -1:
						// before
						break;
					case 0:
						throw new NotImplementedException("This should never occur; the previous iteration should handle a time that is equal to the current head (then tail).");
					case 1:
						throw new NotImplementedException("This should never occur; the previous iterations should handle any time that is ahead of the current iteration.");

					default:
						throw new ArgumentOutOfRangeException();
				}

				return Fit(a, b, time);
			}

			// [negative infinity, first]
			return FitAsHead(dataSet, 1, time);
		}
	}
}
