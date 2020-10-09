using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public interface IServerEvents
	{
		void OnConnectionRequest(Server server, ConnectionRequest request, NetDataWriter rejectionContent);

		void OnClientConnected(Server server, Peer client);

		void OnClientDisconnected(Server server, Peer client, DisconnectInfo info);
	}
}
