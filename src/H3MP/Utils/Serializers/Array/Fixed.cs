using System;

namespace H3MP.Utils
{
	public class FixedArraySerializer<TValue> : ISerializer<TValue[]>
	{
		private readonly ISerializer<TValue> _serializer;
		private readonly int _length;

		public FixedArraySerializer(ISerializer<TValue> serializer, int length)
		{
			_serializer = serializer;
			_length = length;
		}

		public TValue[] Deserialize(ref BitPackReader reader)
		{
			var buffer = new TValue[_length];
			for (var i = 0; i < _length; ++i)
			{
				buffer[i] = _serializer.Deserialize(ref reader);
			}

			return buffer;
		}

		public void Serialize(ref BitPackWriter writer, TValue[] value)
		{
			if (value.Length != _length)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value.Length, "Array length must be equal to the fixed size: " + _length);
			}

			for (var i = 0; i < _length; ++i)
			{
				_serializer.Serialize(ref writer, value[i]);
			}
		}
	}
}
