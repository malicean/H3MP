namespace H3MP.Utils
{
	public readonly struct DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter> : ISerializer<TValue[]>
		where TSerializer : ISerializer<TValue>
		where TLengthSerializer : ISerializer<TLength>
		where TLengthConverter : IConverter<TLength, long>, IConverter<long, TLength>
	{
		private readonly TSerializer _serializer;
		private readonly TLengthSerializer _length;

		private readonly TLengthConverter _lengthConverter;

		public DynamicArraySerializer(TSerializer serializer, TLengthSerializer length, TLengthConverter lengthConverter)
		{
			_serializer = serializer;
			_length = length;
			_lengthConverter = lengthConverter;
		}

		public TValue[] Deserialize(ref BitPackReader reader)
		{
			var length = _lengthConverter.Convert(_length.Deserialize(ref reader));
			var buffer = new TValue[length];

			_serializer.ToArrayFixed(buffer).Deserialize(ref reader);

			return buffer;
		}

		public void Serialize(ref BitPackWriter writer, TValue[] value)
		{
			_length.Serialize(ref writer, _lengthConverter.Convert(value.LongLength));

			_serializer.ToArrayFixed(value).Serialize(ref writer, default);
		}
	}
}
