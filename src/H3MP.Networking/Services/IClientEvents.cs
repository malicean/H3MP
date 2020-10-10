using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Networking
{
	public interface IClientEvents<TClient>
	{
		void OnConnected(TClient client);

		void OnDisconnected(TClient client, DisconnectInfo info);
	}
}
