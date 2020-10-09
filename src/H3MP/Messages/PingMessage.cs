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
		public double ClientTime { get; private set; }

		private PingMessage(double time)
		{
			ClientTime = time;
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
