using BepInEx.Logging;
using H3MP.Networking.Implementations;
using H3MP.Networking.Managers;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace H3MP.Networking
{
	public class MessageClient : MessageManager
	{
		public MessagePeer Server => Listener.Peers.First();

		public MessageClient(ManualLogSource log, Dictionary <Type, MessageDefinition> messageDefinitions, IMessageClientEvents events, Version version, string address, ushort port, Action<NetDataWriter> payload = null) : base(messageDefinitions, new MessageClientListener(log, messageDefinitions, events), version)
		{
			Manager.Start();

			using (WriterPool.Instance.Borrow(out var writer))
			{
				writer.Put(Version);
				payload?.Invoke(writer);

				Manager.Connect(address, port, writer);
			}
		}

		public void Send<TMessage>(TMessage message) where TMessage : INetSerializable
		{
			Server.Send(message);
		}
	}
}
