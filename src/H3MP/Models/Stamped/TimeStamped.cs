namespace H3MP.Models
{
	public static class TimeStampedExtensions
	{
		public static TimeStamped<T> ToTime<T>(this TickTimeStamped<T> @this)
		{
			return new TimeStamped<T>(@this.Stamp.Time, @this.Content);
		}
	}

	public readonly struct TimeStamped<T> : IStamped<double, T>
	{
		public double Stamp { get; }
		public T Content { get; }

		public TimeStamped(double stamp, T content)
		{
			Stamp = stamp;
			Content = content;
		}

		public TimeStamped<TWith> WithContent<TWith>(TWith content)
		{
			return new TimeStamped<TWith>(Stamp, content);
		}

		public override string ToString()
		{
			return "{" + Content + " @ " + Stamp + "}";
		}
	}
}
