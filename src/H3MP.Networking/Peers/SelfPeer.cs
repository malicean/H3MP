using H3MP.Utils;
using LiteNetLib;
using System;
using System.Linq;

namespace H3MP.Networking
{
    internal class SelfPeer<TPeer> : IUpdatable, IDisposable
	{
		public NetManager Manager { get; }

		public SelfPeer(PeerMessageList<TPeer> messages, MessageListener<TPeer> listener)
		{
			Manager = new NetManager(listener)
			{
				AutoRecycle = true,
				ChannelsCount = (byte) (messages.Definitions.Values.Max(x => x.Channel) + 1)
			};
		}

		public void Update()
		{
			Manager.PollEvents();
		}

		public void Dispose()
		{
			if (Manager.IsRunning)
			{
				if (Manager.ConnectedPeersCount > 0) 
				{
					Manager.DisconnectAll();
				}

				Manager.Stop();
			}
		}
	}
}
