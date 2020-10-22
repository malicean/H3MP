using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct Timestamped<TContent> : INetSerializable where TContent : INetSerializable, new()
	{
		public double Timestamp { get; private set; }

		public TContent Content { get; private set; }

		public Timestamped(double timestamp, TContent content)
		{
			Timestamp = timestamp;
			Content = content;
		}

		public void Deserialize(NetDataReader reader)
		{
			Timestamp = reader.GetDouble();
			Content = reader.Get<TContent>();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Timestamp);
			writer.Put(Content);
		}
	}
}
