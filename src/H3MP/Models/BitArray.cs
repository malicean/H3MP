using System;
using LiteNetLib.Utils;

namespace H3MP.Models
{
	public readonly struct BitArray
	{
		public const int BITS_PER_ELEMENT = sizeof(int) * 8;

		private readonly int[] _buffer;
		private readonly int _length; // 536,870,911 B (536 GB). Hope this is enough.

		public int Length => _length;

		public BitArray(int[] buffer, int size)
		{
			_buffer = buffer;
			_length = size;
		}

		public bool this[int index]
		{
			get
			{
				if (index < 0 && _length <= index)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be a non-negative integer that is less than the size of the buffer.");
				}

				var bits = _buffer[index / BITS_PER_ELEMENT];
				var bit = 1 << (index % BITS_PER_ELEMENT);

				return (bits & bit) == bit;
			}
		}
	}
}
