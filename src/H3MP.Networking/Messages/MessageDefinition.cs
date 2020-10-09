using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Networking
{
	public readonly struct MessageDefinition
	{
		public byte ID { get; }

		public byte Channel { get; }

		public DeliveryMethod Delivery { get; }

		public MessageDefinition(byte id, byte channel, DeliveryMethod delivery)
		{
			ID = id;
			Channel = channel;
			Delivery = delivery;
		}

		public void Send<TMessage>(NetPeer peer, NetDataWriter writer, TMessage message) where TMessage : INetSerializable
		{
			writer.Put(ID);
			writer.Put(message);

			peer.Send(writer, Channel, Delivery);
		}
	}
}
