using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace H3MP.Server
{
	public class NetEventListener : INetEventListener
	{
		private readonly Settings _settings;

		public NetEventListener(Settings setings)
		{

		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
			throw new NotImplementedException();
		}

		public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
			throw new NotImplementedException();
		}

		public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
			throw new NotImplementedException();
		}

		public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
		{
			throw new NotImplementedException();
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
			throw new NotImplementedException();
		}

		public void OnPeerConnected(NetPeer peer)
		{
			throw new NotImplementedException();
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			throw new NotImplementedException();
		}
	}
}
