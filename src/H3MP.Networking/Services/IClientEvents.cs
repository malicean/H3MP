using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Networking
{
	public interface IClientEvents
	{
		void OnConnected(Client client);

		void OnDisconnected(Client client, DisconnectInfo info);
	}
}
