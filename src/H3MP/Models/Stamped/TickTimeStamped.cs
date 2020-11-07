namespace H3MP.Models
{
	public readonly struct TickTimeStamped<T> : IStamped<TickTimeStamp, T>
	{
		public TickTimeStamp Stamp { get; }
		public T Content { get; }

		public TickTimeStamped(TickTimeStamp stamp, T content)
		{
			Stamp = stamp;
			Content = content;
		}

		public TickTimeStamped<TWith> WithContent<TWith>(TWith content)
		{
			return new TickTimeStamped<TWith>(Stamp, content);
		}

		public override string ToString()
		{
			return "{" + Content + " @ " + Stamp + "}";
		}
	}
}
