namespace H3MP.Fitting
{
	public class UIntInverseFitter : IInverseFitter<uint>
	{
		public float InverseFit(uint a, uint b, uint value)
		{
			return (float) (value - a) / (b - a);
		}
	}
}
