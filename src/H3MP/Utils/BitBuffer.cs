using System;
using System.Collections;
using System.Collections.Generic;

namespace H3MP.Utils
{
	public struct BitBuffer : IBuffer<bool>
	{
		private const byte BITS_PER_ELEMENT = sizeof(byte) * 8;

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
			throw new NotImplementedException();
		}
	}
}
