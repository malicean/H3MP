using System;
using System.Collections.Generic;

namespace H3MP.Networking
{
    public class PeerMessageList<TPeer>
    {
        public Dictionary<Type, MessageDefinition> Definitions { get; }

        public Dictionary<byte, ReaderHandler<TPeer>> Handlers { get; }

        public PeerMessageList() 
        {
            Definitions = new Dictionary<Type, MessageDefinition>();
            Handlers = new Dictionary<byte, ReaderHandler<TPeer>>();
        }
    }
}