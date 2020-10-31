using H3MP.IO;
using H3MP.Models;

namespace H3MP.Serialization
{
	public class TickstampedSerializer<T> : ISerializer<Tickstamped<T>>
	{
		private readonly ISerializer<uint> _tick;
		private readonly ISerializer<T> _content;

		public TickstampedSerializer(ISerializer<T> content)
		{
			_tick = PrimitiveSerializers.UInt;
			_content = content;
		}

		public Tickstamped<T> Deserialize(ref BitPackReader reader)
		{
			return new Tickstamped<T>
			{
				Tick = _tick.Deserialize(ref reader),
				Content = _content.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, Tickstamped<T> value)
		{
			_tick.Serialize(ref writer, value.Tick);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
