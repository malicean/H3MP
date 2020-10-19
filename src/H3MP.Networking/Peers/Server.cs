using BepInEx.Logging;
using H3MP.Networking.Listeners;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;

namespace H3MP.Networking
{
	public abstract class Server<TServer> : IUpdatable, IDisposable where TServer : Server<TServer>
	{
		private readonly SelfPeer<TServer> _peer;
		private readonly MessageListener<TServer> _listener;

		public int ClientsCount => _listener.PeersCount;

		public IEnumerable<Peer> Clients => _listener.Peers;

		public Server(ManualLogSource log, PeerMessageList<TServer> messages, byte channelsCount, IServerEvents<TServer> events, Version version, IPAddress ipv4, IPAddress ipv6, ushort port)
		{
			var listenerEvents = new ServerListenerEvents<TServer>((TServer) this, log, events, version);
			_listener = new MessageListener<TServer>((TServer) this, messages, log, listenerEvents);

			_peer = new SelfPeer<TServer>(channelsCount, _listener);
			if (!_peer.Manager.Start(ipv4, ipv6, port))
			{
				throw new InvalidOperationException($"Could not bind socket. IPv4: {ipv4}; IPv6: {ipv6}; port: {port}");
			}
		}

		public virtual void Update()
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

		public virtual void Dispose()
		{
			_peer.Dispose();
		}
	}
}
