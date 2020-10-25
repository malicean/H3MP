using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace H3MP.Models
{
	public struct BitStack
	{
		private const int BITS_PER_ELEMENT = sizeof(int) * 8;

		public static BitStack CreateDefault()
		{
			return new BitStack(new List<int>());
		}

		private readonly List<int> _buffer;
		private int _current;
		private int _index;

		public int Length => _buffer.Count - (BITS_PER_ELEMENT - _index); // full elements + ending (unfilled maybe) element

		public BitStack(List<int> list)
		{
			_buffer = list;
			_current = 0;
			_index = 0;
		}

		public void Push(bool value)
		{
			if (_index == BITS_PER_ELEMENT)
			{
				_buffer.Add(_current);
				_index = 0;
			}

			int index = _index++;
			if (value)
			{
				_current |= 1 << index;
			}
		}

		public void CopyTo(NetDataWriter writer)
		{
			writer.Put((byte) _buffer.Count); // implicit +1
			writer.Put((byte) _index);
			foreach (var element in _buffer)
			{
				writer.Put(element);
			}
			writer.Put(_current);
		}
	}
}
