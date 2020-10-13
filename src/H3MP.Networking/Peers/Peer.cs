using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace H3MP.Networking
{
    public class Peer
	{
		private readonly NetPeer _peer;
		private readonly Dictionary<Type, MessageDefinition> _definitions;

		public int ID => _peer.Id;

		public Peer(NetPeer peer, Dictionary<Type, MessageDefinition> definitions)
		{
			_peer = peer;
			_definitions = definitions;
		}

		public void Send<TMessage>(TMessage message) where TMessage : INetSerializable
		{
			if (!_definitions.TryGetValue(typeof(TMessage), out MessageDefinition definition))
			{
				throw new InvalidOperationException("No message definition is declared for type " + typeof(TMessage));
			}

			using (WriterPool.Instance.Borrow(out var writer))
			{
				definition.Send(_peer, writer, message);
			}
		}

        public override int GetHashCode()
        {
            return _peer.Id;
        }

		public override string ToString()
		{
			return $"{_peer.Id} ({_peer.EndPoint})";
		}
	}
}
