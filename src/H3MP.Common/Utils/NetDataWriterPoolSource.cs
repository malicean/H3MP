using LiteNetLib;

namespace H3MP.Common.Utils
{
    public class NetDataWriterPoolSource : IStackPoolSource 
    {
        public NetDataWriter Create()
        {
            return new NetDataWriter();
        }

        public void Clean(NetDataWriter item)
        {
            item.Clear();
        }
    }
}