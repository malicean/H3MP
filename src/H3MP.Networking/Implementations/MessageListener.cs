using BepInEx.Logging;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace H3MP.Networking
{
	public abstract class MessageListener : INetEventListener
	{
		private readonly Dictionary<Type, MessageDefinition> _messageDefinitions;
		private readonly Dictionary<NetPeer, MessagePeer> _peers;
		private readonly MessageReceiver _receiver;

		protected ManualLogSource Log { get; }

		public IEnumerable<MessagePeer> Peers => _peers.Values;

		public int PeersCount => _peers.Count;

		public MessageListener(ManualLogSource log, Dictionary<Type, MessageDefinition> messageDefinitions)
		{
			_messageDefinitions = messageDefinitions;
			_peers = new Dictionary<NetPeer, MessagePeer>();
			_receiver = new MessageReceiver(_peers, messageDefinitions.ToDictionary(x => x.Value.ID, x => x.Value.Handler));

			Log = log;
		}

		public abstract void OnConnectionRequest(ConnectionRequest request);

		public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
			Log.LogError($"Encountered network error with peer {endPoint}: {socketError}");
		}

		public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
		}

		public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
		{
			_receiver.Handle(peer, reader);
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		public void OnPeerConnected(NetPeer rawPeer)
		{
			var peer = new MessagePeer(rawPeer, _messageDefinitions);
			Log.LogInfo($"Peer {peer} connected");

			_peers.Add(rawPeer, peer);

			OnPeerConnected(peer);
		}

		protected virtual void OnPeerConnected(MessagePeer peer) { }

		public void OnPeerDisconnected(NetPeer rawPeer, DisconnectInfo info)
		{
			var peer = _peers[rawPeer];
			Log.LogInfo($"Peer {peer} disconnected ({info.Reason})");

			try
			{
				OnPeerDisconnected(peer, info);
			}
			finally
			{
				_peers.Remove(rawPeer);
			}
		}

		protected virtual void OnPeerDisconnected(MessagePeer peer, DisconnectInfo info) { }
	}
}
