namespace H3MP.Models
{
	public struct QueueTickstamped<T>
	{
		public uint ReceivedTick;
		public uint QueuedTick;
		public T Content;
	}
}
