using System;

namespace H3MP.IO
{
	public struct ByteQueue
	{
		private readonly byte[] _bytes;

		private int _index;

		public bool Done => _index == _bytes.Length;

		public ByteQueue(byte[] bytes)
		{
			_bytes = bytes;
			_index = 0;
		}

		public byte Pop()
		{
			if (Done)
			{
				throw new InvalidOperationException("All available bytes have been popped.");
			}

			return _bytes[_index++];
		}
	}
}
