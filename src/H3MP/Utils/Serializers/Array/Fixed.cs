namespace H3MP.Utils
{
	public readonly struct FixedArraySerializer<TValue, TSerializer> : ISerializer<TValue[]> where TSerializer : ISerializer<TValue>
	{
		private readonly TSerializer _serializer;
		private readonly TValue[] _buffer;

		public FixedArraySerializer(TSerializer serializer, TValue[] buffer)
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
