using H3MP.Common;
using H3MP.Common.Utils;

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
using System;

namespace H3MP.Client
{
	public class NetEventListener : INetEventListener, IUpdatable
	{
		private readonly ManualLogSource _log;
		private readonly Pool<NetDataWriter> _writers;
		private readonly ReceiveHandler _receiveHandler;

		private NetPeer _server;
		public NetPeer Server
		{
			get => _time is null ? null : _server;
			private set
			{
				_time = null;

				_server = value;
			}
		}

		private ServerTime _time;
		public double Time => _time?.Now ?? throw new InvalidOperationException("Listener has not fully connected to a server.");

		public NetEventListener(ManualLogSource log, Pool<NetDataWriter> writers)
		{
			_log = log;
			_writers = writers;

			_receiveHandler = new DictionaryReceiveHandler<ServerMessageType>(x => x.GetMessageType())
			{
				Handlers = new Dictionary<ServerMessageType, ReceiveHandler>
				{
					[ServerMessageType.Pong] = ReceiveFrom<PongMessage>((peer, message) =>
					{
						if (_time is null)
						{
							_time = new ServerTime(_log, _server, _writers, message);

							_log.LogDebug("Established server time.");
						}
						else
						{
							_time.ProcessPong(message);
						}
					}),
					[ServerMessageType.LevelChange] = ReceiveFrom<LevelChangeMessage>((peer, message) =>
					{
						_log.LogDebug($"Loading level: {message.Name}");

						SteamVR_LoadLevel.Begin(message.Name);
					})
				},
				OnFallback = (peer, reader, type) =>
				{
					_log.LogWarning($"Received unknown message from server: {(byte) type} with {reader.AvailableBytes} bytes.");

					return true;
				}
			}.RootHandler;
		}

		private ReceiveHandler ReceiveFrom<TMessage>(MessageHandler<TMessage> handler) where TMessage : INetSerializable, new()
		{
			return handler.ToNetHandler(peer => _log.LogWarning($"Received malformed {typeof(TMessage)} message from server."));
		}

		public void Update()
		{
			_time?.Update();
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
			Server = peer;

			_writers.Borrow(out var writer);
			ServerTime.SendPing(_server, writer);
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			Server = null;

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
