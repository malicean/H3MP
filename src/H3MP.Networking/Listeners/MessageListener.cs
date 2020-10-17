using BepInEx.Logging;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace H3MP.Networking
{
	internal class MessageListener<TPeer> : INetEventListener
	{
		private readonly TPeer _manager;
		private readonly PeerMessageList<TPeer> _messages;
		private readonly ManualLogSource _log;
		private readonly IListenerEvents _events;

		private readonly Dictionary<NetPeer, Peer> _peers;

		public IEnumerable<Peer> Peers => _peers.Values;

		public int PeersCount => _peers.Count;

		public MessageListener(TPeer manager, PeerMessageList<TPeer> messages, ManualLogSource log, IListenerEvents events)
		{
			_manager = manager;
			_messages = messages;
			_log = log;
			_events = events;

			_peers = new Dictionary<NetPeer, Peer>();
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
			_events.OnConnectionRequest(request);
		}

		public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
			_log.LogError($"Encountered network error with peer {endPoint}: {socketError}");
		}

		public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
		}

		public void OnNetworkReceive(NetPeer rawPeer, NetPacketReader reader, DeliveryMethod deliveryMethod)
		{
			var peer = _peers[rawPeer];
			var id = reader.GetByte();

			if (!_messages.Handlers.TryGetValue(id, out var handler))
			{
				_log.LogWarning($"Received unknown message ({id}; {reader.AvailableBytes}B) from {peer}");
			}
			else
			{
				handler(_manager, peer, reader);
			}
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		public void OnPeerConnected(NetPeer rawPeer)
		{
			var peer = new Peer(rawPeer, _messages.Definitions);
			_log.LogInfo($"{peer} connected");

			_peers.Add(rawPeer, peer);

			_events.OnPeerConnected(peer);
		}

		public void OnPeerDisconnected(NetPeer rawPeer, DisconnectInfo info)
		{
			var peer = _peers[rawPeer];
			_log.LogInfo($"{peer} disconnected ({info.Reason})");

			try
			{
				_events.OnPeerDisconnected(peer, info);
			}
			finally
			{
				_peers.Remove(rawPeer);
			}
		}
	}
}
