namespace H3MP.Models
{
	public readonly struct LocalTickstamp
	{
		public readonly double Time;
		public readonly long Tick;

		public LocalTickstamp(double time, long tick)
		{
			Time = time;
			Tick = tick;
		}

		public override string ToString()
		{
			return "(" + Tick + "@" + Time + ")";
		}
	}
}
