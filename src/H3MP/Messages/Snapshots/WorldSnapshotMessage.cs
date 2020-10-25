using H3MP.Extensions;
using H3MP.Models;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace H3MP.Messages
{
    public struct WorldSnapshotMessage : INetSerializable, IDeltable<WorldSnapshotMessage>
	{
		#region Party information

		public Option<Key32> PartyID;
		public Option<JoinSecret> Secret;
		public Option<byte> MaxPlayers;

		#endregion

		public Option<string> Level;

		private Option<byte[]> PlayersLeft;
		private Option<byte[]> PlayersJoined;
		private Option<Dictionary<byte, MoveMessage>> Puppets;

		public void Deserialize(NetDataReader reader)
		{
			var options = new NetOptionReader(reader);

			PartyID = options.Get(r => r.GetKey32());
			Secret = options.Get(r => r.GetJoinSecret());
			MaxPlayers = options.Get(r => r.GetByte());

			Level = options.Get(r => r.GetStringWithByteLength());

			PlayersLeft = options.Get(r => r.GetBytesWithByteLength());
			PlayersJoined = options.Get(r => r.GetBytesWithByteLength());
			Puppets = options.Get(r =>
			{
				var count = r.GetByte();
				var puppets = new Dictionary<byte, MoveMessage>(count);

				for (var i = 0; i < count; ++i)
				{
					puppets.Add(r.GetByte(), r.Get<MoveMessage>());
				}

				return puppets;
			});
		}

		public void Serialize(NetDataWriter writer)
		{
			using (var options = new NetOptionWriter(writer))
			{
				options.Put(PartyID, (w, v) => w.Put(v));
				options.Put(Secret, (w, v) => w.Put(v));
				options.Put(MaxPlayers, (w, v) => w.Put(v));

				options.Put(Level, (w, v) => w.PutStringWithByteLength(v));

				options.Put(PlayersLeft, (w, v) => w.PutBytesWithByteLength(v));
				options.Put(PlayersJoined, (w, v) => w.PutBytesWithByteLength(v));

				options.Put(Puppets, (w, v) =>
				{
					var count = v.Count;
					w.Put(count);

					foreach (var pair in v)
					{
						w.Put(pair.Key);
						w.Put(pair.Value);
					}
				});
			}
		}

		public WorldSnapshotMessage ConsumeDelta(WorldSnapshotMessage head)
		{
			var deltas = new DeltaComparer<WorldSnapshotMessage>(this, head);
			return new WorldSnapshotMessage
			{
				PartyID = deltas.Consume(x => x.PartyID.ToDeltaBinary()),
				Secret = deltas.Consume(x => x.Secret.ToDeltaBinary()),
				MaxPlayers = deltas.Consume(x => x.MaxPlayers.ToDeltaBinary()),

				Level = deltas.Consume(x => x.Level.ToDeltaBinary()),

				PlayersLeft = deltas.Consume(x => x.PlayersLeft.ToDeltaBinary()),
				PlayersJoined = deltas.Consume(x => x.PlayersJoined.ToDeltaBinary()),

				Puppets = deltas.Consume(x => x.Puppets.ToDeltaBinary())
			};
		}

		public WorldSnapshotMessage CreateDelta(WorldSnapshotMessage head)
		{
			throw new NotImplementedException();
		}
	}
}
