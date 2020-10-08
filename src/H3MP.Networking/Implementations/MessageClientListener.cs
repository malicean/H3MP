using BepInEx.Logging;
using LiteNetLib;
using System;
using System.Collections.Generic;

namespace H3MP.Networking.Implementations
{
	public class MessageClientListener : MessageListener
	{
		private readonly IMessageClientEvents _events;

		public MessageClientListener(ManualLogSource log, Dictionary<Type, MessageDefinition> messageDefinitions, IMessageClientEvents events) : base(log, messageDefinitions)
		{
			_events = events;
		}

		public override void OnConnectionRequest(ConnectionRequest request)
		{
			Log.LogWarning($"A connection attempt was made by {request.RemoteEndPoint} while the listener is in client mode.");

			request.RejectForce(); // well that was easy
		}

		protected override void OnPeerConnected(MessagePeer peer)
		{
			_events.OnConnected();
		}

		protected override void OnPeerDisconnected(MessagePeer peer, DisconnectInfo info)
		{
			_events.OnDisconnected(info);
		}
	}
}
