using LiteNetLib;

namespace H3MP.Networking.Listeners
{
	internal interface IListenerEvents
	{
		void OnConnectionRequest(ConnectionRequest request);

		void OnPeerConnected(Peer peer);

		void OnPeerDisconnected(Peer peer, DisconnectInfo info);
	}
}
