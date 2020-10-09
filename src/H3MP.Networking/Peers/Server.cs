using BepInEx.Logging;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;

namespace H3MP.Networking
{
	public class Server : IUpdatable, IDisposable
	{
		private readonly SelfPeer<Server> _peer;
		private readonly MessageListener<Server> _listener;

		public Server(ManualLogSource log, PeerMessageList<Server> messages, IServerEvents events, Version version, ushort port)
		{
			var listenerEvents = new ServerListenerEvents(this, log, events, version);
			_listener = new MessageListener<Server>(this, messages, log, listenerEvents);

			_peer = new SelfPeer<Server>(messages, _listener);
			_peer.Manager.Start(port);
		}

		public void Update()
        {
            _peer.Update();
        }

		public void Broadcast<TMessage>(TMessage message) where TMessage : INetSerializable
		{
			foreach (var peer in _listener.Peers)
			{
				peer.Send(message);
			}
		}

		public void BroadcastExcept<TMessage>(Peer exclude, TMessage message) where TMessage : INetSerializable
		{
			foreach (var peer in _listener.Peers)
			{
				if (peer == exclude)
				{
					continue;
				}

				peer.Send(message);
			}
		}

        public void Dispose()
        {
            _peer.Dispose();
        }
    }
}
