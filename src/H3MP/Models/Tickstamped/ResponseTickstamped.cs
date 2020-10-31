using H3MP.Utils;

namespace H3MP.Models
{
	public struct ResponseTickstamped<T>
	{
		public bool DuplicatedInput;
		public Option<uint> ReceivedTick;
		public Option<uint> QueuedTick;
		public uint SentTick;
		public T Content;
	}
}
