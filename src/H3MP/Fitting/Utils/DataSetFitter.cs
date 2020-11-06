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

		public TValue Fit(IEnumerable<KeyValuePair<TTime, TValue>> dataSet, TTime time)
		{
			using (var enumerator = dataSet.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("No data exists to fit.");
				}

				var latest = enumerator.Current;

				// only 1 data point
				if (!enumerator.MoveNext())
				{
					return latest.Value;
				}

				var a = enumerator.Current;

				// [latest, infinity)
				switch (_timeComparer.Compare(time, a.Key))
				{
					case -1:
						break;
					case 0:
						return latest.Value;
					case 1:
						return Fit(a, latest, time);

					default:
						throw new ArgumentOutOfRangeException();
				}

				// (oldest, latest)
				var b = latest;
				while (enumerator.MoveNext())
				{
					b = a;
					a = enumerator.Current;

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

				// (negative infinity, oldest]
				return Fit(a, b, time);
			}
		}
	}
}
