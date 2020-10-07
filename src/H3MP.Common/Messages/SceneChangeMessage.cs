using LiteNetLib.Utils;

namespace H3MP.Common.Messages
{
	public struct SceneChangeMessage : INetSerializable
	{
		public string Scene { get; private set; }

		public SceneChangeMessage(string scene)
		{
			Scene = scene;
		}

		public void Deserialize(NetDataReader reader)
		{
			Scene = reader.GetString();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(Scene);
		}
	}
}
