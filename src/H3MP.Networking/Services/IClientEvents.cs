using LiteNetLib;

namespace H3MP.Networking
{
    public interface IClientEvents<TClient>
	{
		void OnConnected(TClient client);

		void OnDisconnected(TClient client, DisconnectInfo info);
	}
}
