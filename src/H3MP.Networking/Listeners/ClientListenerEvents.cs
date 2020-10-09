using BepInEx.Logging;
using LiteNetLib;

namespace H3MP.Networking
{
    internal class ClientListenerEvents : IListenerEvents
	{
		private readonly Client _client;
		private readonly ManualLogSource _log;
		private readonly IClientEvents _events;

		public ClientListenerEvents(Client client, ManualLogSource log, IClientEvents events)
		{
			_client = client;
			_log = log;
			_events = events;
		}

        public void OnConnectionRequest(ConnectionRequest request)
        {
            _log.LogWarning($"A connection attempt was made by {request.RemoteEndPoint} while the listener is in client mode.");

			request.RejectForce(); // well that was easy
        }

        public void OnPeerConnected(Peer peer)
        {
            _events.OnConnected(_client);
        }

        public void OnPeerDisconnected(Peer peer, DisconnectInfo info)
        {
            _events.OnDisconnected(_client, info);
        }
    }
}
