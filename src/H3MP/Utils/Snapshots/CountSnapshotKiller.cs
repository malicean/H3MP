using System.Collections.Generic;

namespace H3MP.Utils
{
	public class CountSnapshotKiller<T> : ISnapshotKiller<T>
	{
		public int MaxCount { get; set; }

		public CountSnapshotKiller(int maxCount)
		{
			MaxCount = maxCount;
		}

		public bool CanKill(KeyValuePair<double, T> snapshot, List<KeyValuePair<double, T>> snapshots)
		{
			return snapshots.Count > MaxCount;
		}
	}
}
