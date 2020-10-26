using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Utils
{
	public static class WriterPool
	{
		public static Pool<NetDataWriter> Instance { get; } = new Pool<NetDataWriter>(new NetDataWriterPoolSource());
	}
}
