using BepInEx.Logging;
using H3MP.Utils;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Networking.Managers
{
	public abstract class MessageManager : IUpdatable, IDisposable
	{
		protected NetManager Manager { get; }

		protected Version Version { get; }

		public MessageListener Listener { get; }

		public MessageManager(Dictionary<Type, MessageDefinition> messageDefinitions, MessageListener listener, Version version)
		{
			Version = version;
			Manager = new NetManager(listener)
			{
				AutoRecycle = true,
				ChannelsCount = (byte) (messageDefinitions.Max(x => x.Value.Channel) + 1)
			};

			Listener = listener;
		}

		public void Update()
		{
			Manager.PollEvents();
		}

		public void Dispose()
		{
			if (Manager.IsRunning)
			{
				Manager.Stop();
			}
		}
	}
}
