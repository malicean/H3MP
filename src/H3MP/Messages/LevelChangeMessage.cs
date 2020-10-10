using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct LevelChangeMessage : INetSerializable
	{
		public string Name { get; private set; }

		public LevelChangeMessage(string name)
		{
			Name = name;
		}

		public void Deserialize(NetDataReader reader)
		{
			Name = reader.GetString();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Name);
		}
	}
}
