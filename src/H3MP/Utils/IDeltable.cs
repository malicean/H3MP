namespace H3MP.Utils
{
	public interface IDeltable<TSelf> where TSelf : IDeltable<TSelf>
	{
		/// <summary>
		/// 	Creates a delta that represents the difference between this and the head.
		/// </summary>
		TSelf CreateDelta(TSelf head);

		/// <summary>
		/// 	Consumes this delta to create a new head based on the current head.
		/// </summary>
		TSelf ConsumeDelta(TSelf head);
	}
}
