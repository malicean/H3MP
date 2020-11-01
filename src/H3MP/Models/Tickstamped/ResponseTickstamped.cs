using H3MP.Utils;

namespace H3MP.Models
{
	public struct ResponseTickstamped<T>
	{
		public bool DuplicatedInput;
		public Option<BufferTicks> Buffer;
		public uint SentTick;
		public T Content;
	}
}
