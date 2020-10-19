namespace H3MP.Utils
{
	public class LoopTimer
	{
		public double End { get; private set; }

		public double Duration { get; }

		public bool Done => LocalTime.Now >= End;

		public LoopTimer(double duration)
		{
			Duration = duration;

			Reset();
		}

		public void Cycle()
		{
			End += Duration;
		}

		public void Reset()
		{
			End = LocalTime.Now + Duration;
		}

		public bool TryCycle()
		{
			if (!Done)
			{
				return false;
			}

			Cycle();
			return true;
		}

		public bool TryReset()
		{
			if (!Done)
			{
				return false;
			}

			Reset();
			return true;
		}
	}
}
