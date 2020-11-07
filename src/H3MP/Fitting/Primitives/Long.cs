namespace H3MP.Fitting
{
	public class LongInverseFitter : IInverseFitter<long>
	{
		public float InverseFit(long a, long b, long value)
		{
			return (float) ((value - a) / (b - a));
		}
	}
}
