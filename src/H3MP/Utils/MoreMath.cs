namespace H3MP.Utils
{
	public static class MoreMath
	{
		public static double InverseLinearFit(double a, double b, double value)
		{
			return (value - a) / (b - a);
		}
	}
}
