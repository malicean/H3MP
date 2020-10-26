using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
    public struct Timestamped<TContent> : ISerializer where TContent : ISerializer, new()
	{
		public uint Tick { get; private set; }

		public double Timestamp { get; private set; }

		public TContent Content { get; private set; }

		public Timestamped(uint tick, double timestamp, TContent content)
		{
			Tick = tick;
			Timestamp = timestamp;
			Content = content;
		}

		public void Deserialize(ref BitPackReader reader)
		{
			Tick = reader.Bytes.GetUInt();
			Timestamp = reader.Bytes.GetDouble();
			Content = reader.Get<TContent>();
		}

		public void Serialize(ref BitPackWriter writer)
		{
			writer.Bytes.Put(Tick);
			writer.Bytes.Put(Timestamp);
			writer.Put(Content);
		}
	}
}
