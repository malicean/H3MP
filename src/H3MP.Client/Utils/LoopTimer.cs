using H3MP.Common.Utils;

namespace H3MP.Client.Utils
{
	public class LoopTimer
	{
		public double End { get; private set; }

		public double Duration { get; set; }

		public LoopTimer(double duration)
		{
			Duration = duration;
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
