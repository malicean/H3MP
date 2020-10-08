using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public readonly struct MessageDefinition
	{
		public static MessageDefinition New<TMessage>(byte key, DeliveryMethod delivery, byte channel, MessageHandler<TMessage> handler) where TMessage : INetSerializable, new()
		{
			return new MessageDefinition(key, delivery, channel, (peer, reader) =>
			{
				if (!reader.TryGet<TMessage>(out var message))
				{
					return;
				}

				handler(peer, message);
			});
		}

		public byte ID { get; }

		public DeliveryMethod Delivery { get; }

		public byte Channel { get; }

		public ReaderHandler Handler { get; }

		private MessageDefinition(byte key, DeliveryMethod delivery, byte channel, ReaderHandler handler)
		{
			ID = key;
			Delivery = delivery;
			Channel = channel;
			Handler = handler;
		}

		public void Send<TMessage>(NetPeer peer, NetDataWriter writer, TMessage message) where TMessage : INetSerializable
		{
			writer.Put(ID);
			writer.Put(message);

			peer.Send(writer, Channel, Delivery);
		}
	}
}
