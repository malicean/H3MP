namespace H3MP.Utils
{
	public interface IDifferentiator<TValue, TDelta>
	{
		/// <summary>
		/// 	Creates a delta that represents the difference between this and the head.
		/// </summary>
		Option<TDelta> CreateDelta(TValue now, Option<TValue> baseline);

		/// <summary>
		/// 	Consumes this delta to create a new head based on the current head.
		/// </summary>
		TValue ConsumeDelta(TDelta delta, Option<TValue> now);
	}
}
