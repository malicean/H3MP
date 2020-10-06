using H3MP.Server.Extensions;

using H3MP.Common;
using H3MP.Common.Extensions;
using H3MP.Common.Messages;
using H3MP.Common.Utils;

using System;
using System.Net;
using System.Net.Sockets;

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

		// I don't like passing kernels, but NetManager provides a chicken and egg dependency problem that can be resolved out of the constructor.
		public NetEventListener(Logger log, IConnectionSettings settings, Pool<NetDataWriter> writers, IKernel kernel)
		{
			_log = log;
			_settings = settings;
			_writers = writers;
			_kernel = kernel;
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

			ConnectionData data;
			try
			{
				data = request.Data.Get<ConnectionData>();
			}
			catch
			{
				return ConnectionError.MalformedRequest;
			}

			if (data.ApiVersion != ApiConstants.VERSION)
			{
				return ConnectionError.MismatchedVersion;
			}

			if (data.Passphrase != _settings.Passphrase)
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
				_log.Error(e, "Connection request from {Endpoint} failed.", request.RemoteEndPoint);

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
			switch (reader.GetMessageType())
			{
				case ClientMessageType.Ping:
					{
						PingMessage ping;
						try
						{
							ping = reader.Get<PingMessage>();
						}
						catch
						{
							break;
						}

						var reply = new PongMessage(ping.Time, LocalTime.Now);

						_writers.Borrow(out var writer);
						writer.PutTyped(reply);

						peer.Send(writer, DeliveryMethod.ReliableSequenced);
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
			_log.Information("Connection from {Endpoint}.", peer.EndPoint);
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			_log.Information("Disconnection from {Endpoint}.", peer.EndPoint);
		}
	}
}
