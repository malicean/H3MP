using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct Timestamped<TContent>
	{
		public uint Tick { get; private set; }

		public TContent Content { get; private set; }

		public Timestamped(uint tick, TContent content)
		{
			Tick = tick;
			Content = content;
		}
	}
}
