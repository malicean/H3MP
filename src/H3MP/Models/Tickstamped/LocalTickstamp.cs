namespace H3MP.Models
{
	public readonly struct LocalTickstamp
	{
		public readonly double Time;
		public readonly uint Tick;

		public LocalTickstamp(double time, uint tick)
		{
			Time = time;
			Tick = tick;
		}
	}
}
