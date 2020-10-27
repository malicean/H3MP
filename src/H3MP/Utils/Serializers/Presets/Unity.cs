namespace H3MP.Utils
{
	public static class UnitySerializer
	{
		public static Vector3Serializer<FloatSerializer<UIntSerializer<ByteSerializer>>> Vector3 { get; } = default;
		public static QuaternionSerializer<FloatSerializer<UIntSerializer<ByteSerializer>>> Quaternion { get; } = default;
		public static SmallestThreeQuaternionSerializer<FloatSerializer<UIntSerializer<ByteSerializer>>> SmallestThreeQuaternion { get; } = default;
	}
}
