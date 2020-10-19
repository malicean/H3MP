using H3MP.Networking.Listeners;
using H3MP.Utils;
using LiteNetLib;
using System;

namespace H3MP.Networking
{
    internal class SelfPeer<TPeer> : IUpdatable, IDisposable
	{
		public NetManager Manager { get; }

		public SelfPeer(byte channelsCount, MessageListener<TPeer> listener)
		{
			Manager = new NetManager(listener)
			{
				AutoRecycle = true,
				ChannelsCount = channelsCount
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
