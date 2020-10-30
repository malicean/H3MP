namespace H3MP.Fitting
{
	public static class Fitters
	{
		public static QuaternionFitter Quaternion { get; } = new QuaternionFitter();
		public static Vector3Fitter Vector3 { get; } = new Vector3Fitter();
	}

	public static class InverseFitters
	{
		public static UIntInverseFitter UInt { get; } = new UIntInverseFitter();
	}
}
