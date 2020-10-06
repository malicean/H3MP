using H3MP.Client.Extensions;
using H3MP.Client.Utils;

using H3MP.Common.Extensions;
using H3MP.Common.Messages;

using System.Net;
using System.Net.Sockets;

using BepInEx.Logging;

using LiteNetLib;

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
							pong = reader.Get<PongMessage>();
						}
						catch
						{
							break;
						}

						_time.FinishUpdate(pong);
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
			switch (disconnectInfo.Reason)
			{
				case DisconnectReason.RemoteConnectionClose:
					{
						ConnectionError? reason;
						try
						{
							reason = disconnectInfo.AdditionalData.GetConnectionError();
						}
						catch
						{
							reason = null;
						}

						_logger.LogInfo($"Connection closed by {peer.EndPoint}. Reason: {reason?.ToString() ?? "not available (malformed response)"}");
					}
					break;

				default:
					_logger.LogInfo($"Disconnected from {peer.EndPoint}. Reason: {disconnectInfo.Reason}");
					break;
			}
		}
	}
}
