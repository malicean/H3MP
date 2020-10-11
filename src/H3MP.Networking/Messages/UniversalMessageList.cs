using System;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
    public delegate void ReaderHandler<in TPeer>(TPeer self, Peer peer, NetDataReader reader);

	public delegate void MessageHandler<in TPeer, in TMessage>(TPeer self, Peer peer, TMessage message);

	public class UniversalMessageList<TClient, TServer>
	{
        private readonly ManualLogSource _clientLog;
		private readonly ManualLogSource _serverLog;

        private readonly ManagerMessageListData<TClient> _client;
		private readonly ManagerMessageListData<TServer> _server;

		private byte _maxChannel;

		public PeerMessageList<TClient> Client => _client.Messages;

		public PeerMessageList<TServer> Server => _server.Messages;

		public byte ChannelsCount => (byte) (_maxChannel + 1);

		public UniversalMessageList(ManualLogSource clientLog, ManualLogSource serverLog) 
		{
			_clientLog = clientLog;
			_serverLog = serverLog;

			_client = new ManagerMessageListData<TClient>();
			_server = new ManagerMessageListData<TServer>();
		}

		private ReaderHandler<TPeer> CreateReaderFrom<TPeer, TMessage>(MessageHandler<TPeer, TMessage> handler, ManualLogSource log) where TMessage : INetSerializable, new()
        {
            return (self, peer, reader) => 
			{
				if (!reader.TryGet<TMessage>(out var message))
				{
					log.LogError($"Failed to parse {typeof(TMessage)} from {peer}. Does the message have symmetrical serialization?");
					return;
				}

				handler(self, peer, message);
			};
        }

		private void Add<TSender, TReceiver, TMessage>(ManagerMessageListData<TSender> sender, ManagerMessageListData<TReceiver> receiver, ManualLogSource receiverLog, byte channel, DeliveryMethod delivery, MessageHandler<TReceiver, TMessage> handler) where TMessage : INetSerializable, new()
		{
			var id = sender.ID++;
			var definition = new MessageDefinition(id, channel, delivery);
			var reader = CreateReaderFrom(handler, receiverLog);
			
			sender.Messages.Definitions.Add(typeof(TMessage), definition);
			receiver.Messages.Handlers.Add(id, reader);

			_maxChannel = Math.Max(_maxChannel, channel);
		}

        public UniversalMessageList<TClient, TServer> AddClient<TMessage>(byte channel, DeliveryMethod delivery, MessageHandler<TServer, TMessage> handler) where TMessage : INetSerializable, new()
		{
			Add<TClient, TServer, TMessage>(_client, _server, _serverLog, channel, delivery, handler);

			return this;
		}

		public UniversalMessageList<TClient, TServer> AddServer<TMessage>(byte channel, DeliveryMethod delivery, MessageHandler<TClient, TMessage> handler) where TMessage : INetSerializable, new()
		{
			Add<TServer, TClient, TMessage>( _server, _client, _clientLog, channel, delivery, handler);

			return this;
		}

		private class ManagerMessageListData<TPeer>
		{
			public byte ID;

			public PeerMessageList<TPeer> Messages = new PeerMessageList<TPeer>();
		}
	}
}
