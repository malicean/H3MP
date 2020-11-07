namespace H3MP.Models
{
	public readonly struct TickTimeStamp
	{
		public readonly long Tick;
		public readonly double Time;

		public TickTimeStamp(long tick, double time)
		{
			Tick = tick;
			Time = time;
		}

		public override string ToString()
		{
			return "(" + Tick + "; " + Time + ")";
		}
	}
}
