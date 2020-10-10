using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace H3MP.Networking
{
	internal class ServerListenerEvents<TServer> : IListenerEvents
	{
		private readonly TServer _server;
		private readonly ManualLogSource _log;
		private readonly IServerEvents<TServer> _events;
		private readonly Version _version;

		public ServerListenerEvents(TServer server, ManualLogSource log, IServerEvents<TServer> events, Version version)
		{
			_server = server;
			_log = log;
			_events = events;
			_version = version;
		}

		public void OnConnectionRequestSafe(ConnectionRequest request, NetDataWriter writer)
		{
			var reader = request.Data;

			Version version;
			try
			{
				version = reader.GetVersion();
			}
			catch
			{
				_log.LogWarning($"Connection request from {request.RemoteEndPoint} denied because of a malformed version.");

				writer.Put(ConnectionError.MalformedVersion);
				request.Reject(writer);

				return;
			}

			if (!_version.CompatibleWith(version))
			{
				_log.LogWarning($"Connection request from {request.RemoteEndPoint} denied because of version mismatch (server: {_version}; client: {version})");

				writer.Put(ConnectionError.MismatchedVersion);
				writer.Put(new MismatchedVersionErrorMessage(_version, version));
				request.Reject(writer);

				return;
			}
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
			using (WriterPool.Instance.Borrow(out var writer))
			{
				try
				{
					OnConnectionRequestSafe(request, writer);
				}
				catch (Exception e)
				{
					_log.LogError($"Failed to process connection request from {request.RemoteEndPoint}:\n{e}");

					writer.Reset();
					writer.Put(ConnectionError.InternalError);

					request.RejectForce(writer);

					return;
				}

				writer.Reset();
				writer.Put(ConnectionError.UserDefined);

				_events.OnConnectionRequest(_server, request, writer);
			}
		}

        public void OnPeerConnected(Peer peer)
        {
            _events.OnClientConnected(_server, peer);
        }

        public void OnPeerDisconnected(Peer peer, DisconnectInfo info)
        {
            _events.OnClientDisconnected(_server, peer, info);
        }
    }
}
