namespace H3MP.Utils
{
	public static class ArraySerializerExtensions
	{
		public static FixedArraySerializer<TValue, TSerializer> ToArrayFixed<TValue, TSerializer>(this TSerializer @this, TValue[] buffer) where TSerializer : ISerializer<TValue>
		{
			return new FixedArraySerializer<TValue, TSerializer>(@this, buffer);
		}

		public static DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter> ToDynamicWithLength<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter>(this TSerializer @this, TLengthSerializer length, TLengthConverter lengthConverter)
			where TSerializer : ISerializer<TValue>
			where TLengthSerializer : ISerializer<TLength>
			where TLengthConverter : IConverter<TLength, long>, IConverter<long, TLength>
		{
			return new DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter>(@this, length, lengthConverter);
		}

		public static DynamicArraySerializer<TValue, TSerializer, byte, ByteSerializer, ByteLongConverter> ToDynamicByteLength<TValue, TSerializer>(this TSerializer @this)
			where TSerializer : ISerializer<TValue>
		{
			return @this.ToDynamicWithLength<TValue, TSerializer, byte, ByteSerializer, ByteLongConverter>(default, default);
		}

		public static DynamicArraySerializer<TValue, TSerializer, ushort, UShortSerializer<ByteSerializer>, UShortLongConverter> ToDynamicUShortLength<TValue, TSerializer>(this TSerializer @this)
			where TSerializer : ISerializer<TValue>
		{
			return @this.ToDynamicWithLength<TValue, TSerializer, ushort, UShortSerializer<ByteSerializer>, UShortLongConverter>(default, default);
		}
	}
}
