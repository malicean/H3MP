using BepInEx.Logging;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Linq;

namespace H3MP.Networking
{
	public class Client : IUpdatable, IDisposable
	{
		private readonly SelfPeer<Client> _peer;
		private readonly MessageListener<Client> _listener;

		public Peer Server => _listener.Peers.First();

		public Client(ManualLogSource log, PeerMessageList<Client> messages, IClientEvents events, Version version, string address, ushort port, Action<NetDataWriter> payload = null)
		{
			var listenerEvents = new ClientListenerEvents(this, log, events);
			_listener = new MessageListener<Client>(this, messages, log, listenerEvents);

			_peer = new SelfPeer<Client>(messages, _listener);

			using (WriterPool.Instance.Borrow(out var writer))
			{
				writer.Put(version);
				payload?.Invoke(writer);

				_peer.Manager.Start();
				_peer.Manager.Connect(address, port, writer);
			}
		}

		public void Update()
        {
            _peer.Update();
        }

        public void Dispose()
        {
            _peer.Dispose();
        }
    }
}
