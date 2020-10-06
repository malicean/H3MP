using H3MP.Common;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Ninject;

using LiteNetLib;
using LiteNetLib.Utils;

using Serilog;
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
			_writer = writers;
			_kernel = kernel;
		}

		private void OnConnectionRequestSafe(ConnectionRequest request) 
		{
			if (!_settings.Allowed) 
			{
				
				return;
			}
			else
			{
				var net = _kernel.Get<NetManager>();

				if (net.ConnectedPeersCount < _settings.MaxPeers) 
				{
					if (!(_settings.Key is null))
					{
						request.AcceptIfKey(_settings.Key);
					}
					else 
					{
						request.Accept();
					}
					
					return;
				}
			}
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
			try
			{
				OnConnectionRequestSafe(request);
			}
			catch (Exception e)
			{
				_log.Error(e, "Connection request from {Endpoint} failed.", request.RemoteEndPoint);

				_writers.Get(out var writer);
				writer.Put((byte) ConnectionError.InternalError);
				request.RejectForce(writer);
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
