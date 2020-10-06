using BepInEx.Logging;
using H3MP.Client.Extensions;
using H3MP.Client.Utils;
using H3MP.Common.Extensions;
using H3MP.Common.Messages;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace H3MP.Client
{
	public class NetEventListener : INetEventListener
	{
		private readonly ManualLogSource _logger;
		private readonly NetworkTime _time;

		public NetEventListener(ManualLogSource logger, NetworkTime time)
		{
			_logger = logger;
			_time = time;
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
		}

		public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
		}

		public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
		}

		public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
		{
			switch (reader.GetMessageType())
			{
				case ServerMessageType.Pong:
					{
						PongMessage pong;
						try
						{
							reader.Get<PongMessage>();
						}
						catch
						{
							break;
						}
					}
					break;

				default:
					break;
			}
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		public void OnPeerConnected(NetPeer peer)
		{
			_logger.LogInfo($"Connected to {peer.EndPoint}");
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			_logger.LogInfo($"Disconnected from {peer.EndPoint}");
		}
	}
}
