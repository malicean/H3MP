using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public delegate void ReaderHandler<in TPeer>(TPeer self, Peer peer, NetDataReader reader);

	public delegate void MessageHandler<in TPeer, in TMessage>(TPeer self, Peer peer, TMessage message);

	public class PeerMessageListBuilder
	{
		private readonly ManagerMessageListData<Client> _client;

		private readonly ManagerMessageListData<Server> _server;

		public PeerMessageList<Client> Client => _client.Messages;

		public PeerMessageList<Server> Server => _server.Messages;

		public PeerMessageListBuilder() 
		{
			_client = new ManagerMessageListData<Client>();
			_server = new ManagerMessageListData<Server>();
		}

		private static ReaderHandler<TPeer> CreateReaderFrom<TPeer, TMessage>(MessageHandler<TPeer, TMessage> handler) where TMessage : INetSerializable, new()
        {
            return (self, peer, reader) => 
			{
				if (!reader.TryGet<TMessage>(out var message))
				{
					// TODO: maybe handle this better
					return;
				}

				handler(self, peer, message);
			};
        }

		private void Add<TSender, TReceiver, TMessage>(ManagerMessageListData<TSender> sender, ManagerMessageListData<TReceiver> receiver, byte channel, DeliveryMethod delivery, MessageHandler<TReceiver, TMessage> handler) where TMessage : INetSerializable, new()
		{
			var id = sender.ID++;
			var definition = new MessageDefinition(id, channel, delivery);
			var reader = CreateReaderFrom(handler);
			
			sender.Messages.Definitions.Add(typeof(TMessage), definition);
			receiver.Messages.Handlers.Add(id, reader);
		}

        public PeerMessageListBuilder AddClient<TMessage>(byte channel, DeliveryMethod delivery, MessageHandler<Server, TMessage> handler) where TMessage : INetSerializable, new()
		{
			Add<Client, Server, TMessage>(_client, _server, channel, delivery, handler);

			return this;
		}

		public PeerMessageListBuilder AddServer<TMessage>(byte channel, DeliveryMethod delivery, MessageHandler<Client, TMessage> handler) where TMessage : INetSerializable, new()
		{
			Add<Server, Client, TMessage>( _server, _client, channel, delivery, handler);

			return this;
		}

		private class ManagerMessageListData<TPeer>
		{
			public byte ID;

			public PeerMessageList<TPeer> Messages = new PeerMessageList<TPeer>();
		}
	}
}
