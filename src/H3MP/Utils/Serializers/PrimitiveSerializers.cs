namespace H3MP.Utils
{
	public static class PrimitiveSerializer
	{
		// We can use default on all of the following because the serializers are stateless structs.

		public static BoolSerializer Bool { get; } = default;

		public static ByteSerializer Byte { get; } = default;
		public static SByteSerializer<ByteSerializer> SByte { get; } = default;

		public static UShortSerializer<ByteSerializer> UShort { get; } = default;
		public static ShortSerializer<UShortSerializer<ByteSerializer>> Short { get; } = default;

		public static UIntSerializer<ByteSerializer> UInt { get; } = default;
		public static IntSerializer<UIntSerializer<ByteSerializer>> Int { get; } = default;

		public static ULongSerializer<ByteSerializer> ULong { get; } = default;
		public static LongSerializer<ULongSerializer<ByteSerializer>> Long { get; } = default;

		public static FloatSerializer<UIntSerializer<ByteSerializer>> Float { get; } = default;
		public static DoubleSerializer<ULongSerializer<ByteSerializer>> Double { get; } = default;
		public static DecimalSerializer<ULongSerializer<ByteSerializer>> Decimal { get; } = default;

		public static CharSerializer<UShortSerializer<ByteSerializer>> Char { get; } = default;

		// String is not here because it is a dynamically sized type in networking.
	}

}
