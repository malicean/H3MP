namespace H3MP.Conversion
{
	public static class Converters
	{
		public static ByteUShortConverter ByteUShort { get; } = new ByteUShortConverter();
		public static ByteUIntConverter ByteUInt { get; } = new ByteUIntConverter();
		public static ByteIntConverter ByteInt { get; } = new ByteIntConverter();
		public static ByteULongConverter ByteULong { get; } = new ByteULongConverter();

		public static SByteShortConverter SByteShort { get; } = new SByteShortConverter();
		public static SByteIntConverter SByteInt { get; } = new SByteIntConverter();
		public static SByteLongConverter SByteLong { get; } = new SByteLongConverter();

		public static UShortUIntConverter UShortUInt { get; } = new UShortUIntConverter();
		public static UShortIntConverter UShortInt { get; } = new UShortIntConverter();
		public static UShortULongConverter UShortULong { get; } = new UShortULongConverter();
		public static UShortFloatConverter UShortFloat(float max)
		{
			return new UShortFloatConverter(max);
		}

		public static ShortIntConverter ShortInt { get; } = new ShortIntConverter();
		public static ShortLongConverter ShortLong { get; } = new ShortLongConverter();
		public static ShortFloatConverter ShortFloat(float maxAbs)
		{
			return new ShortFloatConverter(maxAbs);
		}

		public static UIntLongConverter UIntLong { get; } = new UIntLongConverter();
		public static UIntULongConverter UIntULong { get; } = new UIntULongConverter();

		public static IntLongConverter IntLong { get; } = new IntLongConverter();
	}
}
