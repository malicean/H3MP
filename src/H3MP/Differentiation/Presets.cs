namespace H3MP.Differentiation
{
	public static class Differentiators
	{
		public static Vector3Differentiator Vector3 { get; } = new Vector3Differentiator();
		public static QuaternionDifferentiator Quaternion { get; } = new QuaternionDifferentiator();
	}
}
