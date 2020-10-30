using System;

namespace H3MP.IO
{
	public struct BitQueue
	{
		private readonly BitArray _bits;

		private int _index;

		public bool Done => _index == _bits.BitLength;

		public BitQueue(BitArray bits)
		{
			_bits = bits;
			_index = 0;
		}

		public bool Pop()
		{
			if (Done)
			{
				throw new InvalidOperationException("All available bits have been popped.");
			}

			return _bits[_index++];
		}
	}
}
