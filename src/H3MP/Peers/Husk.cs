using H3MP.Messages;
using H3MP.Utils;

namespace H3MP.Peers
{
	public class Husk
	{
		private Snapshots<BodyMessage> _movementSnapshots;

		private Timestamped<BodyMessage> _lastDelta;
		public Timestamped<BodyMessage> LastDelta
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
			var killer = new TimeSnapshotKiller<BodyMessage>(() => LocalTime.Now, maxSnapshotAge);

			_movementSnapshots = new Snapshots<BodyMessage>(killer);
			IsSelf = isSelf;

			LastDelta = Timestamped<BodyMessage>.Now();
		}
	}
}
