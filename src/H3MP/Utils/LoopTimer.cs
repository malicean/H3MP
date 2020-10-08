namespace H3MP.Utils
{
	public class LoopTimer
	{
		public double End { get; private set; }

		public double Duration { get; }

		public LoopTimer(double duration)
		{
			Duration = duration;

			Reset();
		}

		public void Reset()
		{
			End = LocalTime.Now + Duration;
		}

		public bool TryReset()
		{
			if (LocalTime.Now < End)
			{
				return false;
			}

			End += Duration;
			return true;
		}
	}
}
