using H3MP.IO;
using H3MP.Serialization;

namespace H3MP.Messages
{
	public struct TickstampedResponseMessage<T>
	{
		public uint Tick;
		public uint RespondingToTick;
		public T Content;
	}

	public class TickstampedResponseMessageSerializer<T> : ISerializer<TickstampedResponseMessage<T>>
	{
		private readonly ISerializer<uint> _tick;
		private readonly ISerializer<T> _content;

		public TickstampedResponseMessageSerializer(ISerializer<T> content)
		{
			_tick = PrimitiveSerializers.UInt;
			_content = content;
		}

		public TickstampedResponseMessage<T> Deserialize(ref BitPackReader reader)
		{
			return new TickstampedResponseMessage<T>
			{
				Tick = _tick.Deserialize(ref reader),
				RespondingToTick = _tick.Deserialize(ref reader),
				Content = _content.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, TickstampedResponseMessage<T> value)
		{
			_tick.Serialize(ref writer, value.Tick);
			_tick.Serialize(ref writer, value.RespondingToTick);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
