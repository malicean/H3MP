using H3MP.IO;
using H3MP.Models;

namespace H3MP.Serialization
{
	public class LevelInstanceSerializer : ISerializer<LevelInstance>
	{
		private readonly ISerializer<ushort> _id;
		private readonly ISerializer<string> _name;

		public LevelInstanceSerializer()
		{
			_id = PrimitiveSerializers.UShort;
			_name = PrimitiveSerializers.Char.ToString(TruncatedSerializers.ByteAsInt);
		}

		public LevelInstance Deserialize(ref BitPackReader reader)
		{
			var id = _id.Deserialize(ref reader);
			var name = _name.Deserialize(ref reader);

			return new LevelInstance(id, name);
		}

		public void Serialize(ref BitPackWriter writer, LevelInstance value)
		{
			_id.Serialize(ref writer, value.ID);
			_name.Serialize(ref writer, value.Name);
		}
	}
}
