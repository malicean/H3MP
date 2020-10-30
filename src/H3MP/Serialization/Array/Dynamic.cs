using H3MP.IO;

namespace H3MP.Serialization
{
	public class DynamicArraySerializer<TValue> : ISerializer<TValue[]>
	{
		private readonly ISerializer<TValue> _serializer;
		private readonly ISerializer<int> _length;

		public DynamicArraySerializer(ISerializer<TValue> serializer, ISerializer<int> length)
		{
			_serializer = serializer;
			_length = length;
		}

		public TValue[] Deserialize(ref BitPackReader reader)
		{
			var length = _length.Deserialize(ref reader);

			var buffer = new TValue[length];
			for (var i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = _serializer.Deserialize(ref reader);
			}

			return buffer;
		}

		public void Serialize(ref BitPackWriter writer, TValue[] value)
		{
			_length.Serialize(ref writer, value.Length);

			for (var i = 0; i < value.Length; ++i)
			{
				_serializer.Serialize(ref writer, value[i]);
			}
		}
	}
}
