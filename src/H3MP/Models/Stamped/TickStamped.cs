namespace H3MP.Models
{
	public static class TickStampedExtensions
	{
		public static TickStamped<T> ToTick<T>(this TickTimeStamped<T> @this)
		{
			return new TickStamped<T>(@this.Stamp.Tick, @this.Content);
		}
	}

	public readonly struct TickStamped<T> : IStamped<long, T>
	{
		public long Stamp { get; }
		public T Content { get; }

		public TickStamped(long stamp, T content)
		{
			Stamp = stamp;
			Content = content;
		}

		public TickStamped<TWith> WithContent<TWith>(TWith content)
		{
			return new TickStamped<TWith>(Stamp, content);
		}

		public override string ToString()
		{
			return "{" + Content + " @ " + Stamp + "}";
		}
	}
}
