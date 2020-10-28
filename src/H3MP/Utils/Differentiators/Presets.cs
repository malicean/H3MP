namespace H3MP.Utils
{
	public static class Differentiators
	{
		public static Vector3Differentiator Vector3 { get; } = new Vector3Differentiator();
		public static QuaternionDifferentiator Quaternion { get; } = new QuaternionDifferentiator();
	}
}
