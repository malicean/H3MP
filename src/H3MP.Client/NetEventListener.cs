using H3MP.Common;
using H3MP.Client.Extensions;
using H3MP.Client.Utils;

using H3MP.Common.Extensions;
using H3MP.Common.Messages;

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using BepInEx.Logging;

using LiteNetLib;
using LiteNetLib.Utils;

using UnityEngine.SceneManagement;

namespace H3MP.Client
{
	public class NetEventListener : INetEventListener
	{
		private readonly ManualLogSource _log;
		private readonly ServerTime _time;

		private readonly ReceiveHandler _receiveHandler;

		private ReceiveHandler ReceiveFrom<TMessage>(MessageHandler<TMessage> handler) where TMessage : INetSerializable, new()
		{
			return handler.ToNetHandler(peer => _log.LogWarning($"Received malformed {typeof(TMessage)} message from server."));
		}

		public NetEventListener(ManualLogSource log, ServerTime time)
		{
			_log = log;
			_time = time;

			_receiveHandler = new DictionaryReceiveHandler<ServerMessageType>(x => x.GetMessageType())
			{
				Handlers = new Dictionary<ServerMessageType, ReceiveHandler>
				{
					[ServerMessageType.Pong] = ReceiveFrom<PongMessage>((peer, message) => _time.FinishUpdate(message)),
					[ServerMessageType.SceneChange] = ReceiveFrom<SceneChangeMessage>((peer, message) =>
					{
						var scene = SceneManager.GetSceneByName(message.Scene);

						_log.LogDebug($"Loading scene. Name: {scene.name}; Build Index: {scene.buildIndex}; Path: {scene.path}");

						SteamVR_LoadLevel.Begin(scene.name);
					})
				},
				OnFallback = (peer, reader, type) =>
				{
					_log.LogWarning($"Received unknown message from server: {(byte) type} with {reader.AvailableBytes} bytes.");

					return true;
				}
			}.RootHandler;
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
			_receiveHandler(peer, reader);
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		public void OnPeerConnected(NetPeer peer)
		{
			_log.LogInfo($"Connected to {peer.EndPoint}");
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

						_log.LogInfo($"Connection closed by {peer.EndPoint}. Reason: {reason?.ToString() ?? "not available (malformed response)"}");
					}
					break;

				default:
					_log.LogInfo($"Disconnected from {peer.EndPoint}. Reason: {disconnectInfo.Reason}");
					break;
			}
		}
	}
}
