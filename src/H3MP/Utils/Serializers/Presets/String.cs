namespace H3MP.Utils
{
	public static class StringSerializer
	{
		public static StringSerializer<TLength, CharSerializer<UShortSerializer<ByteSerializer>>, TLengthSerializer, TLengthConverter> WithLength<TLength, TLengthSerializer, TLengthConverter>(TLengthSerializer length, TLengthConverter lengthConverter)
			where TLengthSerializer : ISerializer<TLength>
			where TLengthConverter : IConverter<TLength, long>, IConverter<long, TLength>
		{
			return new StringSerializer<TLength, CharSerializer<UShortSerializer<ByteSerializer>>, TLengthSerializer, TLengthConverter>(length, lengthConverter, default);
		}

		public static StringSerializer<byte, CharSerializer<UShortSerializer<ByteSerializer>>, ByteSerializer, ByteLongConverter> ByteLength { get; } =
			StringSerializer.WithLength<byte, ByteSerializer, ByteLongConverter>(default, default);

		public static StringSerializer<ushort, CharSerializer<UShortSerializer<ByteSerializer>>, UShortSerializer<ByteSerializer>, UShortLongConverter> UShortLength { get; } =
			StringSerializer.WithLength<ushort, UShortSerializer<ByteSerializer>, UShortLongConverter>(default, default);
	}
}
