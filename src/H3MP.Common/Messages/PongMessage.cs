using H3MP.Common.Messages;
using LiteNetLib.Utils;

namespace H3MP.Common.Messages
{
	public struct PongMessage : INetSerializable
	{
		public double SeedTime { get; private set; }

		public double ReplyTime { get; private set; }

		public PongMessage(double seedTime, double replyTime)
		{
			SeedTime = seedTime;
			ReplyTime = replyTime;
		}

		public void Deserialize(NetDataReader reader)
		{
			SeedTime = reader.GetDouble();
			ReplyTime = reader.GetDouble();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(SeedTime);
			writer.Put(ReplyTime);
		}
	}
}
