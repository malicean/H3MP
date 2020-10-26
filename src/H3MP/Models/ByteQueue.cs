namespace H3MP.Models
{
	public struct ByteQueue
	{
		private readonly byte[] _bytes;

		private int _index;

		public ByteQueue(byte[] bytes)
		{
			_bytes = bytes;
			_index = 0;
		}

		public byte Pop()
		{
			return _bytes[_index++];
		}
	}
}
