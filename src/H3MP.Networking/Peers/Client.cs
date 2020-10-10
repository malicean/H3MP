using BepInEx.Logging;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Linq;
using System.Net;

namespace H3MP.Networking
{
	public abstract class Client<TClient> : IUpdatable, IDisposable where TClient : Client<TClient>
	{
		private readonly SelfPeer<TClient> _peer;
		private readonly MessageListener<TClient> _listener;

		public Peer Server { get; }

		public Client(ManualLogSource log, PeerMessageList<TClient> messages, IClientEvents<TClient> events, Version version, IPEndPoint endpoint, Action<NetDataWriter> payload = null)
		{
			var listenerEvents = new ClientListenerEvents<TClient>((TClient) this, log, events);
			_listener = new MessageListener<TClient>((TClient) this, messages, log, listenerEvents);

			_peer = new SelfPeer<TClient>(messages, _listener);

			using (WriterPool.Instance.Borrow(out var writer))
			{
				writer.Put(version);
				payload?.Invoke(writer);

				_peer.Manager.Start();
				_peer.Manager.Connect(endpoint, writer);
			}

			Server = _listener.Peers.First();
		}

		public virtual void Update()
        {
            _peer.Update();
        }

        public virtual void Dispose()
        {
            _peer.Dispose();
        }
    }
}
