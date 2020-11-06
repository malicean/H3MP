using System;

namespace H3MP.Timing
{
	public interface IFrameClock
	{
		double Time { get; }
		double DeltaTime { get; }

		event Action Elapsed;
	}
}
