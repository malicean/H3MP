namespace H3MP.Utils
{
	public static class TruncatedSerializers
	{
		public static ISerializer<ushort> ByteAsUShort { get; }  = new ConverterSerializer<ushort, byte>(PrimitiveSerializers.Byte, Converters.ByteUShort, Converters.ByteUShort);
		public static ISerializer<uint> ByteAsUInt { get; } = new ConverterSerializer<uint, byte>(PrimitiveSerializers.Byte, Converters.ByteUInt, Converters.ByteUInt);
		public static ISerializer<int> ByteAsInt { get; } = new ConverterSerializer<int, byte>(PrimitiveSerializers.Byte, Converters.ByteInt, Converters.ByteInt);
		public static ISerializer<ulong> ByteAsULong { get; } = new ConverterSerializer<ulong, byte>(PrimitiveSerializers.Byte, Converters.ByteULong, Converters.ByteULong);

		public static ISerializer<short> SByteAsShort { get; } = new ConverterSerializer<short, sbyte>(PrimitiveSerializers.SByte, Converters.SByteShort, Converters.SByteShort);
		public static ISerializer<int> SByteAsInt { get; } = new ConverterSerializer<int, sbyte>(PrimitiveSerializers.SByte, Converters.SByteInt, Converters.SByteInt);
		public static ISerializer<long> SByteAsLong { get; } = new ConverterSerializer<long, sbyte>(PrimitiveSerializers.SByte, Converters.SByteLong, Converters.SByteLong);

		public static ISerializer<uint> UShortAsUInt { get; } = new ConverterSerializer<uint, ushort>(PrimitiveSerializers.UShort, Converters.UShortUInt, Converters.UShortUInt);
		public static ISerializer<int> UShortAsInt { get; } = new ConverterSerializer<int, ushort>(PrimitiveSerializers.UShort, Converters.ShortUInt, Converters.ShortUInt);
		public static ISerializer<ulong> UShortAsULong { get; } = new ConverterSerializer<ulong, ushort>(PrimitiveSerializers.UShort, Converters.UShortULong, Converters.UShortULong);

		public static ISerializer<int> ShortAsInt { get; } = new ConverterSerializer<int, short>(PrimitiveSerializers.Short, Converters.ShortInt, Converters.ShortInt);
		public static ISerializer<long> ShortAsLong { get; } = new ConverterSerializer<long, short>(PrimitiveSerializers.Short, Converters.ShortLong, Converters.ShortLong);

		public static ISerializer<ulong> UIntAsULong { get; } = new ConverterSerializer<ulong, uint>(PrimitiveSerializers.UInt, Converters.UIntULong, Converters.UIntULong);

		public static ISerializer<long> IntAsLong { get; } = new ConverterSerializer<long, int>(PrimitiveSerializers.Int, Converters.IntLong, Converters.IntLong);

		public static ISerializer<float> FloatWithMax(float maxAbs, ISerializer<short> serializer)
		{
			var converter = new ShortFloatConverter(maxAbs);

			return new ConverterSerializer<float, short>(serializer, converter, converter);
		}

		public static ISerializer<float> UFloatWithMax(float max, ISerializer<ushort> serializer)
		{
			var converter = new UShortFloatConverter(max);

			return new ConverterSerializer<float, ushort>(serializer, converter, converter);
		}
	}

}
