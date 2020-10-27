namespace H3MP.Utils
{
	public static class ConverterSerializerExtensions
	{
		public static ConverterSerializer<TValue, TConverted, TConverter, TSerializer> ToConverter<TValue, TConverted, TConverter, TSerializer>(this TSerializer @this, TConverter converter)
			where TConverter : IConverter<TValue, TConverted>, IConverter<TConverted, TValue>
			where TSerializer : ISerializer<TConverted>
		{
			return new ConverterSerializer<TValue, TConverted, TConverter, TSerializer>(@this, converter);
		}
	}

	public readonly struct ConverterSerializer<TValue, TConverted, TConverter, TSerializer> : ISerializer<TValue>
		where TConverter : IConverter<TValue, TConverted>, IConverter<TConverted, TValue>
		where TSerializer : ISerializer<TConverted>
	{
		private readonly TSerializer _serializer;
		private readonly TConverter _converter;

		public ConverterSerializer(TSerializer serializer, TConverter converter)
		{
			_serializer = serializer;
			_converter = converter;
		}

		public TValue Deserialize(ref BitPackReader reader)
		{
			return _converter.Convert(_serializer.Deserialize(ref reader));
		}

		public void Serialize(ref BitPackWriter writer, TValue value)
		{
			_serializer.Serialize(ref writer, _converter.Convert(value));
		}
	}
}
