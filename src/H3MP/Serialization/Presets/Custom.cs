namespace H3MP.Serialization
{
	public static class CustomSerializers
	{
		public static Key32Serializer Key32 { get; } = new Key32Serializer();
		public static JoinSecretSerializer JoinSecret { get; } = new JoinSecretSerializer();
		public static BufferTicksSerializer BufferTicks { get; } = new BufferTicksSerializer();
	}
}
