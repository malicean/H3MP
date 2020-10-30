namespace H3MP.Timing
{
	public class LoopTimer
	{
		public double End { get; private set; }

		public double Interval { get; }

		public LoopTimer(double interval, double now)
		{
			Interval = interval;

			Reset(now);
		}

		public bool IsDone(double now)
		{
			return now >= End;
		}

		public void Cycle()
		{
			End += Interval;
		}

		public void Reset(double now)
		{
			End = now + Interval;
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
