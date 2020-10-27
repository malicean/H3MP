using System;

namespace H3MP.Utils
{
	public readonly struct StringSerializer<TLength, TCharSerializer, TLengthSerializer, TLengthConverter> : ISerializer<string>
		where TCharSerializer : ISerializer<char>
		where TLengthSerializer : ISerializer<TLength>
		where TLengthConverter : IConverter<TLength, long>, IConverter<long, TLength>
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
