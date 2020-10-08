using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct PingMessage : INetSerializable
	{
		/// <summary>
		///		The client's time at the instant the message was sent.
		/// </summary>
		public double ClientTime { get; private set; }

		public PingMessage()
		{
			ClientTime = LocalTime.Now;
		}

		public void Deserialize(NetDataReader reader)
		{
			ClientTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ClientTime);
		}
	}
}
