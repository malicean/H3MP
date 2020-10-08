using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace H3MP.Networking
{
	public interface IMessageServerEvents
	{
		void OnConnectionRequest(ConnectionRequest request, NetDataWriter rejectionContent);

		void OnPeerConnected(MessagePeer peer);

		void OnPeerDisconnected(MessagePeer peer, DisconnectInfo info);
	}
}
