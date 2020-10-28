namespace H3MP.Utils
{
	public static class PrimitiveSerializers
	{
		public static ISerializer<bool> Bool { get; } = new BoolSerializer();

		public static ISerializer<byte> Byte { get; } = new ByteSerializer();
		public static ISerializer<sbyte> SByte { get; } = new SByteSerializer();

		public static ISerializer<ushort> UShort { get; } = new UShortSerializer();
		public static ISerializer<short> Short { get; } = new ShortSerializer();

		public static ISerializer<char> Char { get; } = new CharSerializer();

		public static ISerializer<uint> UInt { get; } = new UIntSerializer();
		public static ISerializer<int> Int { get; } = new IntSerializer();

		public static ISerializer<ulong> ULong { get; } = new ULongSerializer();
		public static ISerializer<long> Long { get; } = new LongSerializer();

		public static ISerializer<float> Float { get; } = new FloatSerializer();
		public static ISerializer<double> Double { get; } = new DoubleSerializer();
		public static ISerializer<decimal> Decimal { get; } = new DecimalSerializer();

		// String is not here because it is a dynamically sized type.
	}
}
