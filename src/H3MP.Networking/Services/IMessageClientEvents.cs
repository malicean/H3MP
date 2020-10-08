using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Networking
{
	public interface IMessageClientEvents
	{
		void OnConnected();

		void OnDisconnected(DisconnectInfo info);
	}
}
