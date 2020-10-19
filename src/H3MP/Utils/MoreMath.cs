namespace H3MP.Utils
{
	public static class MoreMath
	{
		public static double InverseLerp(double a, double b, double value)
        {
            return (value - a) / (b - a);
        }
	}
}
