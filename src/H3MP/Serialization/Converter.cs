using H3MP.Conversion;
using H3MP.IO;

namespace H3MP.Serialization
{
	public class ConverterSerializer<TValue, TConverted> : ISerializer<TValue>
	{
		private readonly ISerializer<TConverted> _serializer;
		private readonly IConverter<TValue, TConverted> _convertTo;
		private readonly IConverter<TConverted, TValue> _convertFrom;

		public ConverterSerializer(ISerializer<TConverted> serializer, IConverter<TValue, TConverted> convertTo, IConverter<TConverted, TValue> convertFrom)
		{
			_serializer = serializer;
			_convertTo = convertTo;
			_convertFrom = convertFrom;
		}

		public TValue Deserialize(ref BitPackReader reader)
		{
			return _convertFrom.Convert(_serializer.Deserialize(ref reader));
		}

		public void Serialize(ref BitPackWriter writer, TValue value)
		{
			_serializer.Serialize(ref writer, _convertTo.Convert(value));
		}
	}
}
