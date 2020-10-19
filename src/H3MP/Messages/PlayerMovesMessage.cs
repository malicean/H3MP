using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace H3MP.Messages
{
	public struct PlayerMovesMessage : INetSerializable
	{
		public Dictionary<byte, Timestamped<PlayerTransformsMessage>> Players { get; private set; }

		public PlayerMovesMessage(Dictionary<byte, Timestamped<PlayerTransformsMessage>> players)
		{
			Players = players;
		}

		public void Deserialize(NetDataReader reader)
		{
			var playerCount = reader.GetByte();
			Players = new Dictionary<byte, Timestamped<PlayerTransformsMessage>>(playerCount);

			for (var i = 0; i < playerCount; ++i)
			{
				Players.Add(reader.GetByte(), reader.Get<Timestamped<PlayerTransformsMessage>>());
			}
		}

		public void Serialize(NetDataWriter writer)
		{
			var playerCount = (byte) Players.Count;
			writer.Put(playerCount);

			foreach (KeyValuePair<byte, Timestamped<PlayerTransformsMessage>> player in Players)
			{
				writer.Put(player.Key);
				writer.Put(player.Value);
			}
		}
	}
}
