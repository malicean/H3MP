using H3MP.IO;
using H3MP.Models;
using H3MP.Utils;

namespace H3MP.Serialization
{
	public class ResponseTickstampedSerializer<T> : ISerializer<ResponseTickstamped<T>>
	{
		private readonly ISerializer<bool> _duplicatedInput;
		private readonly ISerializer<uint> _tick;
		private readonly ISerializer<Option<uint>> _optionalTick;
		private readonly ISerializer<T> _content;

		public ResponseTickstampedSerializer(ISerializer<T> content)
		{
			_duplicatedInput = PrimitiveSerializers.Bool;
			_tick = PrimitiveSerializers.UInt;
			_optionalTick = PrimitiveSerializers.UInt.ToOption();
			_content = content;
		}

		public ResponseTickstamped<T> Deserialize(ref BitPackReader reader)
		{
			return new ResponseTickstamped<T>
			{
				DuplicatedInput = _duplicatedInput.Deserialize(ref reader),
				QueuedTick = _optionalTick.Deserialize(ref reader),
				ReceivedTick = _optionalTick.Deserialize(ref reader),
				SentTick = _tick.Deserialize(ref reader),
				Content = _content.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, ResponseTickstamped<T> value)
		{
			_duplicatedInput.Serialize(ref writer, value.DuplicatedInput);
			_optionalTick.Serialize(ref writer, value.QueuedTick);
			_optionalTick.Serialize(ref writer, value.ReceivedTick);
			_tick.Serialize(ref writer, value.SentTick);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
