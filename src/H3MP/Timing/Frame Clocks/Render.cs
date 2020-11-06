using System;
using System.Diagnostics;

namespace H3MP.Timing
{
	public class RenderFrameClock : IFrameClock
	{
		private readonly Stopwatch _watch;

		public double Time { get; private set; }

		public double DeltaTime { get; private set; }

		public event Action Elapsed;

		public RenderFrameClock(Stopwatch watch)
		{
			_watch = watch;
		}

		public void Update()
		{
			var now = _watch.Elapsed.TotalSeconds;

			DeltaTime = now - Time;
			Time = now;

			Elapsed?.Invoke();
		}
	}
}
