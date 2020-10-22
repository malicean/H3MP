using H3MP.Messages;
using H3MP.Utils;

namespace H3MP.Peers
{
	public class Husk
	{
		private Snapshots<MoveMessage> _movementSnapshots;

		private Timestamped<MoveMessage> _lastDelta;
		public Timestamped<MoveMessage> LastDelta
		{
			get => _lastDelta;
			set
			{
				_movementSnapshots.Push(value.Timestamp, value.Content);
				_lastDelta = value;
			}
		}

		public bool IsSelf { get; }

		public Husk(bool isSelf, double maxSnapshotAge)
		{
			var killer = new TimeSnapshotKiller<MoveMessage>(() => LocalTime.Now, maxSnapshotAge);

			_movementSnapshots = new Snapshots<MoveMessage>(killer);
			IsSelf = isSelf;

			LastDelta = Timestamped<MoveMessage>.Now();
		}
	}
}
