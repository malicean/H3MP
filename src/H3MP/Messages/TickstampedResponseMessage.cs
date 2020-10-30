using H3MP.IO;
using H3MP.Serialization;
using H3MP.Utils;

namespace H3MP.Messages
{
	public struct TickstampedResponseMessage<T>
	{
		public uint Tick;
		public Option<uint> RespondingToTick;
		public T Content;
	}

	public class TickstampedResponseMessageSerializer<T> : ISerializer<TickstampedResponseMessage<T>>
	{
		private readonly ISerializer<uint> _tick;
		private readonly ISerializer<Option<uint>> _respondingToTick;
		private readonly ISerializer<T> _content;

		public TickstampedResponseMessageSerializer(ISerializer<T> content)
		{
			_tick = PrimitiveSerializers.UInt;
			_respondingToTick = PrimitiveSerializers.UInt.ToOption();
			_content = content;
		}

		public TickstampedResponseMessage<T> Deserialize(ref BitPackReader reader)
		{
			return new TickstampedResponseMessage<T>
			{
				Tick = _tick.Deserialize(ref reader),
				RespondingToTick = _respondingToTick.Deserialize(ref reader),
				Content = _content.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, TickstampedResponseMessage<T> value)
		{
			_tick.Serialize(ref writer, value.Tick);
			_respondingToTick.Serialize(ref writer, value.RespondingToTick);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
