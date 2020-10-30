using System;
using LiteNetLib.Utils;

namespace H3MP.IO
{
    public struct BitBuffer : IBuffer<bool>
	{
		private const byte BITS_PER_ELEMENT = sizeof(byte) * 8;

		public static void Serialize(NetDataWriter writer, BitBuffer value)
		{
			writer.Put((byte) value._buffer.Length);
			writer.Put((byte) (BITS_PER_ELEMENT - value._index));

			var bits = value._buffer.Populated;
			writer.Put(bits.Array, bits.Offset, bits.Count);
		}

		private Buffer<byte> _buffer;
		private byte _current;
		private byte _index;

		public int Length => _buffer.Length * BITS_PER_ELEMENT - (BITS_PER_ELEMENT - _index);

		public int Capacity => _buffer.Capacity * BITS_PER_ELEMENT;

		public ArraySegment<byte> Populated => _buffer.Populated;

		public BitBuffer(int capacity)
		{
			_buffer = new Buffer<byte>(capacity);
			_current = 0;
			_index = 0;
		}

		public void Push(bool value)
		{
			if (_index == BITS_PER_ELEMENT)
			{
				_buffer.Push(_current);
				_index = 0;
			}

			int index = _index++;
			if (value)
			{
				_current |= (byte) (1 << index);
			}
		}

		public void Clear()
		{
			_buffer.Clear();
		}
	}
}
