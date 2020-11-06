using H3MP.Conversion;

namespace H3MP.Serialization
{
	public static class PackedSerializers
	{
		public static ISerializer<ushort> UShort { get; } = TruncatedSerializers.ByteAsUShort.ToBranching(x => x > byte.MaxValue, PrimitiveSerializers.UShort);
		public static ISerializer<short> Short { get; } = TruncatedSerializers.SByteAsShort.ToBranching(x => x > sbyte.MaxValue, PrimitiveSerializers.Short);

		public static ISerializer<uint> UInt { get; } = TruncatedSerializers.ByteAsUInt.ToBranching(x => x > byte.MaxValue, TruncatedSerializers.UShortAsUInt)
															.ToBranching(x => x > ushort.MaxValue, PrimitiveSerializers.UInt);
		public static ISerializer<int> Int { get; } = TruncatedSerializers.SByteAsInt.ToBranching(x => x < sbyte.MinValue || sbyte.MaxValue < x, TruncatedSerializers.ShortAsInt)
															.ToBranching(x => x < short.MinValue || short.MaxValue < x, PrimitiveSerializers.Int);

		public static ISerializer<ulong> ULong { get; } = TruncatedSerializers.ByteAsULong.ToBranching(x => x > byte.MaxValue, TruncatedSerializers.UShortAsULong)
															.ToBranching(x => x > ushort.MaxValue, TruncatedSerializers.UIntAsULong.ToBranching(x => x > uint.MaxValue, PrimitiveSerializers.ULong));
		public static ISerializer<long> Long { get; } = TruncatedSerializers.SByteAsLong.ToBranching(x => x < sbyte.MinValue || sbyte.MaxValue < x, TruncatedSerializers.ShortAsLong)
															.ToBranching(x => x < short.MinValue || short.MaxValue < x, TruncatedSerializers.IntAsLong.ToBranching(x => x < int.MinValue || int.MaxValue < x, PrimitiveSerializers.Long));

		public static ISerializer<float> UFloat(float max)
		{
			return TruncatedSerializers.UFloatWithBounds(max, PrimitiveSerializers.UShort).ToBranching(x => x > max, PrimitiveSerializers.Float);
		}

		public static ISerializer<float> Float(float min, float max)
		{
			return TruncatedSerializers.FloatWithBounds(min, max, PrimitiveSerializers.Short).ToBranching(x => x < min || max < x, PrimitiveSerializers.Float);
		}
	}
}
