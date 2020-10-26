using System;

namespace H3MP.Utils
{
	public static class StringSerializer
	{
		public static StringSerializer<TLength, CharSerializer<UShortSerializer<ByteSerializer>>, TLengthSerializer, TLengthConverter> WithLength<TLength, TLengthSerializer, TLengthConverter>(TLengthSerializer length, TLengthConverter lengthConverter)
			where TLengthSerializer : ISerializer<TLength>
			where TLengthConverter : IConverter<TLength, int>, IConverter<int, TLength>
		{
			return new StringSerializer<TLength, CharSerializer<UShortSerializer<ByteSerializer>>, TLengthSerializer, TLengthConverter>(length, lengthConverter, default);
		}

		public static StringSerializer<byte, CharSerializer<UShortSerializer<ByteSerializer>>, ByteSerializer, ByteIntConverter> ByteLength { get; } =
			StringSerializer.WithLength<byte, ByteSerializer, ByteIntConverter>(default, default);

		public static StringSerializer<ushort, CharSerializer<UShortSerializer<ByteSerializer>>, UShortSerializer<ByteSerializer>, UShortIntConverter> UShortLength { get; } =
			StringSerializer.WithLength<ushort, UShortSerializer<ByteSerializer>, UShortIntConverter>(default, default);

		public static StringSerializer<int, CharSerializer<UShortSerializer<ByteSerializer>>, IntSerializer<UIntSerializer<ByteSerializer>>, IntIntConverter> IntLength { get; } =
			StringSerializer.WithLength<int, IntSerializer<UIntSerializer<ByteSerializer>>, IntIntConverter>(default, default);
	}

	public readonly struct StringSerializer<TLength, TCharSerializer, TLengthSerializer, TLengthConverter> : ISerializer<string>
		where TCharSerializer : ISerializer<char>
		where TLengthSerializer : ISerializer<TLength>
		where TLengthConverter : IConverter<TLength, int>, IConverter<int, TLength>
	{
		private readonly TLengthSerializer _length;

		private readonly TLengthConverter _lengthConverter;

		private readonly TCharSerializer _char;

		public StringSerializer(TLengthSerializer length, TLengthConverter lengthConverter, TCharSerializer @char)
		{
			_length = length;
			_lengthConverter = lengthConverter;
			_char = @char;
		}

		public string Deserialize(ref BitPackReader reader)
		{
			var chars = _char.ToDynamicWithLength<char, TCharSerializer, TLength, TLengthSerializer, TLengthConverter>(_length, _lengthConverter).Deserialize(ref reader);

			return new string(chars, 0, chars.Length);
		}

		public void Serialize(ref BitPackWriter writer, string value)
		{
			_char.ToDynamicWithLength<char, TCharSerializer, TLength, TLengthSerializer, TLengthConverter>(_length, _lengthConverter).Serialize(ref writer, value.ToCharArray());
		}
	}
}
