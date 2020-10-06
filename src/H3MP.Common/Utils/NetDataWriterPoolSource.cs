using LiteNetLib.Utils;

namespace H3MP.Common.Utils
{
    public class NetDataWriterPoolSource : IPoolSource<NetDataWriter>
    {
        public NetDataWriter Create()
        {
            return new NetDataWriter();
        }

        public void Clean(NetDataWriter item)
        {
            item.Reset();
        }
    }
}
