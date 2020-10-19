using System;
using System.Collections.Generic;

namespace H3MP.Utils
{
	public class TimeSnapshotKiller<T> : ISnapshotKiller<T>
	{
		private Func<double> _timeGetter;

		public double Window { get; set; }

		public TimeSnapshotKiller(Func<double> timeGetter, double window)
		{
			_timeGetter = timeGetter;

			Window = window;
		}

		public bool CanKill(KeyValuePair<double, T> snapshot, List<KeyValuePair<double, T>> snapshots)
		{
			var now = _timeGetter();
			var age = now - snapshot.Key;

			return age > Window;
		}
	}
}
