using System;

namespace H3MP.Utils
{
	public static class StringSerializerExtensions
	{
		public static StringSerializer<byte> ByteLength
	}

	public readonly struct StringSerializer<TLength, TLengthSerializer, TLengthConverter> : ISerializer<string> where TLengthSerializer : ISerializer<TLength> where TLengthConverter : IConverter<TLength, int>
	{
		private readonly TLengthSerializer _lengthSerializer;
		private readonly TLengthConverter _lengthConverter;
		private readonly TCharSerializer _charSerializer;

		public string Deserialize(ref BitPackReader reader)
		{
			var length = _lengthConverter.Convert(_lengthSerializer.Deserialize(ref reader));

			var chars = new char[length];
			for (var i = 0; i < )
		}

		public void Serialize(ref BitPackWriter writer, string value)
		{
			throw new System.NotImplementedException();
		}
	}
}
