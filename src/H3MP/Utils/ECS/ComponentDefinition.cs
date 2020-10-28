namespace H3MP.Utils
{
	public class ComponentDefinition<TValue, TDelta>
	{
		public readonly IFitter<TValue> Fitter;
		public readonly IDifferentiator<TValue, TDelta> Differentiator;
		public readonly ISerializer<TDelta> Serializer;

		public ComponentDefinition(IFitter<TValue> fitter, IDifferentiator<TValue, TDelta> differentiator, ISerializer<TDelta> serializer)
		{
			Fitter = fitter;
			Differentiator = differentiator;
			Serializer = serializer;
		}
	}
}
