using Discord;
using H3MP.Models;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct InitMessage : INetSerializable
	{
		// I would make this the Party struct but I want verification so someone can't put "cock and ball torture" for the party ID.
		public Key32 ID { get; private set; }

		public JoinSecret Secret { get; private set; }

		public byte MaxSize { get; private set; }

		public LevelChangeMessage Level { get; private set; }

		public PlayerJoinMessage[] Players { get; private set; }
		
		public InitMessage(Key32 id, JoinSecret secret, byte maxSize, LevelChangeMessage level, PlayerJoinMessage[] players)
		{
			ID = id;
			Secret = secret;
			MaxSize = maxSize;
			Level = level;
			Players = players;
		}

		public void Deserialize(NetDataReader reader)
		{
			ID = reader.GetKey32();
			Secret = reader.GetJoinSecret();
			MaxSize = reader.GetByte();
			Level = reader.Get<LevelChangeMessage>();
			Players = new PlayerJoinMessage[reader.GetByte()];
			for (var i = 0; i < Players.Length; ++i)
			{
				Players[i] = reader.Get<PlayerJoinMessage>();
			}
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ID);
			writer.Put(Secret);
			writer.Put(MaxSize);
			writer.Put(Level);
			writer.Put((byte) Players.Length);
			foreach (var player in Players)
			{
				writer.Put(player);
			}
		}
	}
}