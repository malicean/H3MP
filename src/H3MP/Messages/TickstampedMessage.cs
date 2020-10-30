using H3MP.IO;
using H3MP.Serialization;

namespace H3MP.Messages
{
	public struct TickstampedMessage<T>
	{
		public uint Tick;
		public T Content;
	}

	public class TickstampedMessageSerializer<T> : ISerializer<TickstampedMessage<T>>
	{
		private readonly ISerializer<uint> _tick;
		private readonly ISerializer<T> _content;

		public TickstampedMessageSerializer(ISerializer<T> content)
		{
			_tick = PrimitiveSerializers.UInt;
			_content = content;
		}

		public TickstampedMessage<T> Deserialize(ref BitPackReader reader)
		{
			return new TickstampedMessage<T>
			{
				Tick = _tick.Deserialize(ref reader),
				Content = _content.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, TickstampedMessage<T> value)
		{
			_tick.Serialize(ref writer, value.Tick);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
