namespace H3MP.Fitting
{
	public class DoubleInverseFitter : IInverseFitter<double>
	{
		public float InverseFit(double a, double b, double value)
		{
			return (float) ((value - a) / (b - a));
		}
	}
}
