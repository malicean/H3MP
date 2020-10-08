using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public static class WriterPool
	{
		public static Pool<NetDataWriter> Instance { get; }
	}
}
