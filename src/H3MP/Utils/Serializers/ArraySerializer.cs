namespace H3MP.Utils
{
	public static class ArraySerializerExtensions
	{
		public static FixedArraySerializer<TValue, TSerializer> ToFixedArray<TValue, TSerializer>(this TSerializer @this, TValue[] buffer) where TSerializer : ISerializer<TValue>
		{
			return new FixedArraySerializer<TValue, TSerializer>(@this, buffer);
		}

		public static DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter> ToDynamicWithLength<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter>(this TSerializer @this, TLengthSerializer length, TLengthConverter lengthConverter)
			where TSerializer : ISerializer<TValue>
			where TLengthSerializer : ISerializer<TLength>
			where TLengthConverter : IConverter<TLength, int>, IConverter<int, TLength>
		{
			return new DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter>(@this, length, lengthConverter);
		}

		public static DynamicArraySerializer<TValue, TSerializer, byte, ByteSerializer, ByteIntConverter> ToDynamicByteLength<TValue, TSerializer>(this TSerializer @this)
			where TSerializer : ISerializer<TValue>
		{
			return new DynamicArraySerializer<TValue, TSerializer, byte, ByteSerializer, ByteIntConverter>(@this, default, default);
		}

		public static DynamicArraySerializer<TValue, TSerializer, ushort, UShortSerializer<ByteSerializer>, UShortIntConverter> ToDynamicUShortLength<TValue, TSerializer>(this TSerializer @this)
			where TSerializer : ISerializer<TValue>
		{
			return new DynamicArraySerializer<TValue, TSerializer, ushort, UShortSerializer<ByteSerializer>, UShortIntConverter>(@this, default, default);
		}

		public static DynamicArraySerializer<TValue, TSerializer, int, IntSerializer<UIntSerializer<ByteSerializer>>, IntIntConverter> ToDynamicIntLength<TValue, TSerializer>(this TSerializer @this)
			where TSerializer : ISerializer<TValue>
		{
			return new DynamicArraySerializer<TValue, TSerializer, int, IntSerializer<UIntSerializer<ByteSerializer>>, IntIntConverter>(@this, default, default);
		}
	}

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

	public readonly struct DynamicArraySerializer<TValue, TSerializer, TLength, TLengthSerializer, TLengthConverter> : ISerializer<TValue[]>
		where TSerializer : ISerializer<TValue>
		where TLengthSerializer : ISerializer<TLength>
		where TLengthConverter : IConverter<TLength, int>, IConverter<int, TLength>
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

			_serializer.ToFixedArray(buffer).Deserialize(ref reader);

			return buffer;
		}

		public void Serialize(ref BitPackWriter writer, TValue[] value)
		{
			_length.Serialize(ref writer, _lengthConverter.Convert(value.Length));

			_serializer.ToFixedArray(value).Serialize(ref writer, default);
		}
	}
}
