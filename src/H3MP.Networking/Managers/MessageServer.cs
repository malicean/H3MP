using BepInEx.Logging;
using H3MP.Networking.Implementations;
using H3MP.Networking.Managers;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace H3MP.Networking
{
	public class MessageServer : MessageManager
	{
		public MessageServer(ManualLogSource log, Dictionary<Type, MessageDefinition> messageDefinitions, IMessageServerEvents events, Version version, ushort port) : base(messageDefinitions, new MessageServerListener(log, messageDefinitions, events, version), version)
		{
			Manager.Start(port);
		}

		public void Broadcast<TMessage>(TMessage message) where TMessage : INetSerializable
		{
			foreach (var peer in Listener.Peers)
			{
				peer.Send(message);
			}
		}

		public void BroadcastExcept<TMessage>(MessagePeer exclude, TMessage message) where TMessage : INetSerializable
		{
			foreach (var peer in Listener.Peers)
			{
				if (peer == exclude)
				{
					continue;
				}

				peer.Send(message);
			}
		}
	}
}
