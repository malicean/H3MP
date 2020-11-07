using H3MP.IO;
using H3MP.Models;

namespace H3MP.Serialization
{
	public class TickStampedSerializer<T> : ISerializer<TickStamped<T>>
	{
		private readonly ISerializer<long> _stamp;
		private readonly ISerializer<T> _content;

		public TickStampedSerializer(ISerializer<T> content)
		{
			_stamp = PrimitiveSerializers.Long;
			_content = content;
		}

		public TickStamped<T> Deserialize(ref BitPackReader reader)
		{
			var stamp = _stamp.Deserialize(ref reader);
			var content = _content.Deserialize(ref reader);

			return new TickStamped<T>(stamp, content);
		}

		public void Serialize(ref BitPackWriter writer, TickStamped<T> value)
		{
			_stamp.Serialize(ref writer, value.Stamp);
			_content.Serialize(ref writer, value.Content);
		}
	}
}
