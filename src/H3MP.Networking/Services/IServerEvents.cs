using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public interface IServerEvents<TServer>
	{
		void OnConnectionRequest(TServer server, ConnectionRequest request, NetDataWriter rejectionContent);

		void OnClientConnected(TServer server, Peer client);

		void OnClientDisconnected(TServer server, Peer client, DisconnectInfo info);
	}
}
