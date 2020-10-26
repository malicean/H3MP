using System;

namespace H3MP.Utils
{
	public struct Buffer<T> : IBuffer<T>
	{
		private T[] _buffer;
		private int _index;

		public int Length => _index;

		public int Capacity => _buffer.Length;

		public ArraySegment<T> Populated => new ArraySegment<T>(_buffer, 0, _index);

		public Buffer(int capacity)
		{
			_buffer = new T[capacity];
			_index = 0;
		}

		public void Push(T value)
		{
			var index = _index++;
			if (index == _buffer.Length) // end of buffer
			{
				Array.Resize(ref _buffer, _buffer.Length * 2);
			}

			_buffer[index] = value;
		}

		public void Clear()
		{
			_index = 0;
		}
	}
}
