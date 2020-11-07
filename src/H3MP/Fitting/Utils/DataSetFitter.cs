using System;
using System.Collections.Generic;
using H3MP.Models;

namespace H3MP.Fitting
{
	public class DataSetFitter<TStamped, TTime, TValue> where TStamped : IStamped<TTime, TValue>
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

		private TValue Fit(TStamped a, TStamped b, TTime time)
		{
			var t = _timeInverseFitter.InverseFit(a.Stamp, b.Stamp, time);
			return _valueFitter.Fit(a.Content, b.Content, t);
		}

		public TValue Fit(IEnumerable<TStamped> dataSet, TTime time)
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
					return latest.Content;
				}

				var a = enumerator.Current;

				// [latest, infinity)
				switch (_timeComparer.Compare(time, a.Stamp))
				{
					case -1:
						break;
					case 0:
						return latest.Content;
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

					switch (_timeComparer.Compare(time, a.Stamp))
					{
						case -1:
							continue;
						case 0:
							return a.Content;
						case 1:
							// ahead
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					switch (_timeComparer.Compare(time, b.Stamp))
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
