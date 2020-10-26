namespace H3MP.Utils
{
	public static class ArraySerializerExtensions
	{
		public static ArraySerializer<TValue, TSerializer> ToArray<TValue, TSerializer>(this TSerializer @this, TValue[] buffer) where TSerializer : ISerializer<TValue>
		{
			return new ArraySerializer<TValue, TSerializer>(@this, buffer);
		}
	}

	public readonly struct ArraySerializer<TValue, TSerializer> : ISerializer<TValue[]> where TSerializer : ISerializer<TValue>
	{
		private readonly TSerializer _serializer;
		private readonly TValue[] _buffer;

		public ArraySerializer(TSerializer serializer, TValue[] buffer)
		{
			_serializer = serializer;
			_buffer = buffer;
		}

		public TValue[] Deserialize(ref BitPackReader reader)
		{
			for (var i = 0; i < _buffer.Length; ++i)
			{
				_buffer[i] = _serializer.Deserialize(ref reader);
			}

			return _buffer;
		}

		public void Serialize(ref BitPackWriter writer, TValue[] value)
		{
			for (var i = 0; i < _buffer.Length; ++i)
			{
				_serializer.Serialize(ref writer, _buffer[i]);
			}
		}
	}
}
