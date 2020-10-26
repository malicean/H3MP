namespace H3MP.Models
{
	public struct BitQueue
	{
		private readonly BitArray _bits;

		private int _index;

		public BitQueue(BitArray bits)
		{
			_bits = bits;
			_index = 0;
		}

		public bool Pop()
		{
			return _bits[_index++];
		}
	}
}
