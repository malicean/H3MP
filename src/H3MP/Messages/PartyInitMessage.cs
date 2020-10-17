using Discord;
using H3MP.Models;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP.Messages
{
	public struct PartyInitMessage : INetSerializable
	{
		// I would make this the Party struct but I want verification so someone can't put "cock and ball torture" for the party ID.
		public Key32 ID { get; private set; }

		public PartySize Size { get; private set; }

		public JoinSecret Secret { get; private set; }
		
		public PartyInitMessage(Key32 id, PartySize size, JoinSecret secret)
		{
			ID = id;
			Size = size;
			Secret = secret;
		}

		public void Deserialize(NetDataReader reader)
		{
			ID = reader.GetKey32();
			Size = reader.GetPartySize();
			Secret = reader.GetJoinSecret();
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(ID);
			writer.Put(Size);
			writer.Put(Secret);
		}
	}
}