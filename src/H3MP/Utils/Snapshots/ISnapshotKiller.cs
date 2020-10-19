using System.Collections.Generic;

namespace H3MP.Utils
{
	public interface ISnapshotKiller<T>
	{
		bool CanKill(KeyValuePair<double, T> snapshot, List<KeyValuePair<double, T>> snapshots);
	}
}
