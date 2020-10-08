using System;
using System.Collections.Generic;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public delegate void ReaderHandler(MessagePeer peer, NetDataReader reader);
	public delegate void MessageHandler<in TMessage>(MessagePeer peer, TMessage message);
	public delegate bool UnknownHandler(MessagePeer peer, byte key, NetDataReader reader);

	internal class MessageReceiver
	{
		private readonly Dictionary<NetPeer, MessagePeer> _peers;
		private readonly Dictionary<byte, ReaderHandler> _handlers;

		public event UnknownHandler OnUnknownMessage;

		public MessageReceiver(Dictionary<NetPeer, MessagePeer> peers, Dictionary<byte, ReaderHandler> handlers)
		{
			_peers = peers;
			_handlers = handlers;
		}

		public void Handle(NetPeer rawPeer, NetDataReader reader)
		{
			var peer = _peers[rawPeer];
			var key = reader.GetByte();

			if (!_handlers.TryGetValue(key, out var handler))
			{
				OnUnknownMessage?.Invoke(peer, key, reader);
			}
			else
			{
				handler(peer, reader);
			}
		}
	}
}
