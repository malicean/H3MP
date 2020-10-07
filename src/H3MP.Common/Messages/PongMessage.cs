using LiteNetLib.Utils;

namespace H3MP.Common.Messages
{
	public struct PongMessage : INetSerializable
	{
		/// <summary>
		///		The client's time at the instant the message was sent.
		/// </summary>
		public double ClientTime { get; private set; }

		/// <summary>
		///		The server's time at the instant the server responded.
		/// </summary>
		public double ServerTime { get; private set; }

		public PongMessage(double seedTime, double replyTime)
		{
			ClientTime = seedTime;
			ServerTime = replyTime;
		}

		public void Deserialize(NetDataReader reader)
		{
			ClientTime = reader.GetDouble();
			ServerTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ClientTime);
			writer.Put(ServerTime);
		}
	}
}
