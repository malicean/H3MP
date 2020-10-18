using System;
using System.Collections.Generic;

namespace H3MP.Utils
{
	public class Snapshots<T> where T : ILinearFittable<T>
	{
		// Queue but indexable
		private readonly List<KeyValuePair<double, T>> _snapshots;
		private readonly ISnapshotKiller<T> _killer;

		public Snapshots(ISnapshotKiller<T> killer)
		{
			_snapshots = new List<KeyValuePair<double, T>>();
			_killer = killer;
		}

		public void Push(double timestamp, T value)
		{
			// Remove all old snapshots while keeping 1 if needed to perform the a fit
			while (_snapshots.Count > 1 && _killer.CanKill(_snapshots[0], _snapshots))
			{
				_snapshots.RemoveAt(0);
			}

			_snapshots.Add(new KeyValuePair<double, T>(timestamp, value));
		}

		private static float LerpT(double timestamp, double start, double end)
		{
			return (float) ((timestamp - end) / (start - end));
		}

		public T this[double timestamp]
		{
			get
			{
				if (_snapshots.Count == 0)
				{
					throw new InvalidOperationException("No snapshots exist.");
				}

				if (_snapshots.Count == 1)
				{
					return _snapshots[0].Value;
				}

				var newest = _snapshots[_snapshots.Count - 1];

				// Extrapolate forward.
				if (newest.Key < timestamp)
				{
					var secondNewest = _snapshots[_snapshots.Count - 2];
					var t = LerpT(timestamp, secondNewest.Key, newest.Key);

					return secondNewest.Value.Fit(newest.Value, t);
				}

				var oldest = _snapshots[0];

				// Extrapolate backward.
				if (timestamp < oldest.Key)
				{
					var secondOldest = _snapshots[1];
					var t = LerpT(timestamp, oldest.Key, secondOldest.Key);

					return secondOldest.Value.Fit(oldest.Value, t);
				}

				// Interpolate
				for (var i = _snapshots.Count - 1; i > 0; --i)
				{
					var newer = _snapshots[i];
					var older = _snapshots[i - 1];

					if (timestamp == newer.Key)
					{
						return newer.Value;
					}
					else if (older.Key < timestamp && timestamp < newer.Key)
					{
						var t = LerpT(timestamp, older.Key, newer.Key);

						return older.Value.Fit(newer.Value, t);
					}
				}

				return oldest.Value;
			}
		}
	}
}