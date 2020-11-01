using H3MP.IO;
using H3MP.Models;

namespace H3MP.Serialization
{
	public class BufferTicksSerializer : ISerializer<BufferTicks>
	{
		private readonly ISerializer<uint> _tick;

		public BufferTicksSerializer()
        {
			_tick = PrimitiveSerializers.UInt;
		}

		public BufferTicks Deserialize(ref BitPackReader reader)
		{
			return new BufferTicks
			{
				Received = _tick.Deserialize(ref reader),
				Queued = _tick.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, BufferTicks value)
		{
			_tick.Serialize(ref writer, value.Received);
			_tick.Serialize(ref writer, value.Queued);
		}
	}
}
