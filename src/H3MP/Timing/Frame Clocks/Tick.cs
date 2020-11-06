using System;
using System.Diagnostics;

namespace H3MP.Timing
{
	public class TickFrameClock : IFrameClock
	{
		private readonly Stopwatch _watch;

		private double _oldFixedDeltaTime;
		private double _upt;
		private double _updatesRemaining;

		public long Tick { get; private set; }

		public double Time { get; private set; }

		public double DeltaTime { get; }

		public event Action Elapsed;

		public TickFrameClock(Stopwatch watch, double deltaTime)
		{
			_watch = watch;

			DeltaTime = deltaTime;

			RecalculateUpt(UnityEngine.Time.fixedDeltaTime);
		}

		private void RecalculateUpt(double fixedDeltaTime)
		{
			var upt = DeltaTime / fixedDeltaTime;

			_updatesRemaining += upt - _upt;
			_upt = upt;
			_oldFixedDeltaTime = fixedDeltaTime;
		}

		public void FixedUpdate()
		{
			double fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
			if (fixedDeltaTime != _oldFixedDeltaTime)
			{
				RecalculateUpt(fixedDeltaTime);
			}

			if ((_updatesRemaining -= 1) > -1)
			{
				return;
			}

			var now = _watch.Elapsed.TotalSeconds;

			_updatesRemaining += _upt;

			++Tick;
			Time = now;

			Elapsed?.Invoke();
		}
	}
}
