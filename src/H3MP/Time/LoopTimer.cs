namespace H3MP.Time
{
	public class LoopTimer
	{
		public double End { get; private set; }

		public double Duration { get; }

		public LoopTimer(double interval, double now)
		{
			Duration = interval;

			Reset(now);
		}

		public bool IsDone(double now)
		{
			return now >= End;
		}

		public void Cycle()
		{
			End += Duration;
		}

		public void Reset(double now)
		{
			End = now + Duration;
		}

		public bool TryCycle(double now)
		{
			if (!IsDone(now))
			{
				return false;
			}

			Cycle();
			return true;
		}

		public bool TryReset(double now)
		{
			if (!IsDone(now))
			{
				return false;
			}

			Reset(now);
			return true;
		}
	}
}
