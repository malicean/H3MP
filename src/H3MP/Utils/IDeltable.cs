namespace H3MP.Utils
{
	public interface IDeltable<TSelf, TDelta> where TSelf : IDeltable<TSelf, TDelta>
	{
		TDelta InitialDelta { get; }

		/// <summary>
		/// 	Creates a delta that represents the difference between this and the head.
		/// </summary>
		Option<TDelta> CreateDelta(TSelf baseline);

		/// <summary>
		/// 	Consumes this delta to create a new head based on the current head.
		/// </summary>
		TSelf ConsumeDelta(TDelta delta);
	}
}
