using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct PingMessage : INetSerializable
	{
		public static PingMessage Now => new PingMessage(LocalTime.Now);

		/// <summary>
		///		The client's time at the instant the message was sent.
		/// </summary>
		public double Timestamp { get; private set; }

		public PingMessage(double time)
		{
			Timestamp = time;
		}

		public void Deserialize(NetDataReader reader)
		{
			Timestamp = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Timestamp);
		}
	}
}
