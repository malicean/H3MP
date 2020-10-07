using H3MP.Server.Extensions;

using H3MP.Common;
using H3MP.Common.Extensions;
using H3MP.Common.Messages;
using H3MP.Common.Utils;

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Ninject;

using LiteNetLib;
using LiteNetLib.Utils;

using Serilog.Core;

namespace H3MP.Server
{
	public class NetEventListener : INetEventListener
	{
		private readonly Logger _log;
		private readonly IConnectionSettings _settings;
		private readonly Pool<NetDataWriter> _writers;
		private readonly IKernel _kernel;

		private readonly ReceiveHandler _receiveHandler;

		private ReceiveHandler ReceiveFrom<TMessage>(MessageHandler<TMessage> handler) where TMessage : INetSerializable, new()
		{
			return handler.ToNetHandler(peer => _log.Warning("Received malformed {Type} message from peer {Endpoint}.", typeof(TMessage), peer.EndPoint));
		}

		// I don't like passing kernels, but NetManager provides a chicken and egg dependency problem that can be resolved out of the constructor.
		public NetEventListener(Logger log, IConnectionSettings settings, Pool<NetDataWriter> writers, IKernel kernel)
		{
			_log = log;
			_settings = settings;
			_writers = writers;
			_kernel = kernel;

			_receiveHandler = new DictionaryReceiveHandler<ClientMessageType>(x => x.GetMessageType())
			{
				Handlers = new Dictionary<ClientMessageType, ReceiveHandler>
				{
					[ClientMessageType.Ping] = ReceiveFrom<PingMessage>((peer, message) =>
					{
						var now = LocalTime.Now;
						var reply = new PongMessage(message.Time, now);

						_writers.Borrow(out var writer);
						writer.PutTyped(reply);

						peer.Send(writer, DeliveryMethod.ReliableSequenced);
					})
				},
				OnFallback = (peer, reader, type) =>
				{
					_log.Warning("Received unknown message from peer {Endpoint}: {Type} with {MessageLength} bytes.", peer.EndPoint, (byte) type, reader.AvailableBytes);

					return true;
				}
			}.RootHandler;
		}

		private ConnectionError? OnConnectionRequestSafe(ConnectionRequest request) 
		{
			if (!_settings.Allowed) 
			{
				return ConnectionError.Closed;
			}
			
			var net = _kernel.Get<NetManager>();

			if (net.ConnectedPeersCount >= _settings.MaxPeers)
			{
				return ConnectionError.Full;
			}

			if (!request.Data.TryCatchGet<ConnectionRequestMessage>(out var message))
			{
				return ConnectionError.MalformedRequest;
			}
			
			if (message.ApiVersion != ApiConstants.VERSION)
			{
				return ConnectionError.MismatchedVersion;
			}

			if (message.Passphrase != _settings.Passphrase)
			{
				return ConnectionError.MismatchedPassphrase;
			}

			return null;
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
			ConnectionError? error;
			try
			{
				error = OnConnectionRequestSafe(request);
			}
			catch (Exception e)
			{
				_log.Error(e, "Failed connection request from {Endpoint}.", request.RemoteEndPoint);

				_writers.Borrow(out var writer);

				writer.Put(ConnectionError.InternalError);
				request.RejectForce(writer);

				return;
			}

			if (error is null)
			{
				request.Accept();
			}
			else
			{
				_log.Information("Denied connection request from {Endpoint} with reason: {Reason}", request.RemoteEndPoint, error.Value);

				_writers.Borrow(out var writer);

				writer.Put(error.Value);
				request.Reject(writer);
			}
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
			_log.Information("Connected to {Endpoint}.", peer.EndPoint);

			_writers.Borrow(out var writer);
			writer.PutTyped(new SceneChangeMessage(_settings.Scene));

			peer.Send(writer, DeliveryMethod.ReliableSequenced);
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			_log.Information("Disconnected from {Endpoint} because {Reason}.", peer.EndPoint, disconnectInfo.Reason);
		}
	}
}
