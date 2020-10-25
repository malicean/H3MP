using H3MP.Extensions;
using H3MP.Models;
using H3MP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace H3MP.Messages
{
	public struct WorldSnapshotMessage : IPackedSerializable, IDeltable<WorldSnapshotMessage, WorldSnapshotMessage>, IEquatable<WorldSnapshotMessage>
	{
		#region Party information

		public Option<Key32> PartyID;
		public Option<JoinSecret> Secret;
		public Option<byte> MaxPlayers;

		#endregion

		public Option<string> Level;

		public Option<byte[]> PlayersLeft;
		public Option<byte[]> PlayersJoined;
		public Option<Dictionary<byte, MoveMessage>> Puppets;

		public WorldSnapshotMessage InitialDelta => this;

		public void Deserialize(ref BitPackReader reader)
		{
			PartyID = reader.GetOption((ref BitPackReader r) => r.Bytes.GetKey32());
			Secret = reader.GetOption((ref BitPackReader r) => r.Bytes.GetJoinSecret());
			MaxPlayers = reader.GetOption((ref BitPackReader r) => r.Bytes.GetByte());

			Level = reader.GetOption((ref BitPackReader r) => r.Bytes.GetStringWithByteLength());

			PlayersLeft = reader.GetOption((ref BitPackReader r) => r.Bytes.GetBytesWithByteLength());
			PlayersJoined = reader.GetOption((ref BitPackReader r) => r.Bytes.GetBytesWithByteLength());
			Puppets = reader.GetOption((ref BitPackReader r) =>
				r.GetDictionaryWithByteCount((ref BitPackReader r2) => r2.Bytes.GetByte(), (ref BitPackReader r2) => r2.Get<MoveMessage>()));
		}

		public void Serialize(ref BitPackWriter writer)
		{
			writer.Put(PartyID, (ref BitPackWriter w, Key32 v) => w.Bytes.Put(v));
			writer.Put(Secret, (ref BitPackWriter w, JoinSecret v) => w.Bytes.Put(v));
			writer.Put(MaxPlayers, (ref BitPackWriter w, byte v) => w.Bytes.Put(v));

			writer.Put(Level, (ref BitPackWriter w, string v) => w.Bytes.PutStringWithByteLength(v));

			writer.Put(PlayersLeft, (ref BitPackWriter w, byte[] v) => w.Bytes.Put(v));
			writer.Put(PlayersJoined, (ref BitPackWriter w, byte[] v) => w.Bytes.Put(v));
			writer.Put(Puppets, (ref BitPackWriter w, Dictionary<byte, MoveMessage> v) =>
				w.PutDictionaryWithByteLength(v, (ref BitPackWriter w2, byte v2) => w2.Bytes.Put(v2), (ref BitPackWriter w2, MoveMessage v2) => w2.Put(v2)));
		}

		public Option<WorldSnapshotMessage> CreateDelta(WorldSnapshotMessage baseline)
		{
			var deltas = new DeltaCreator<WorldSnapshotMessage>(this, baseline);
			var delta = new WorldSnapshotMessage
			{
				PartyID = deltas.Create(x => x.PartyID, x => x.ToEqualityDelta()),
				Secret = deltas.Create(x => x.Secret, x => x.ToEqualityDelta()),
				MaxPlayers = deltas.Create(x => x.MaxPlayers, x => x.ToEqualityDelta()),

				Level = deltas.Create(x => x.Level, x => x.ToEqualityDelta()),

				PlayersLeft = deltas.Create(x => x.PlayersLeft, x => x),
				PlayersJoined = deltas.Create(x => x.PlayersJoined, x => x),

				Puppets = deltas.Create(x => x.Puppets, x => x)
			};

			return delta.Equals(default)
				? Option.None<WorldSnapshotMessage>()
				: Option.Some(delta);
		}

		public WorldSnapshotMessage ConsumeDelta(WorldSnapshotMessage delta)
		{
			var deltas = new DeltaConsumer<WorldSnapshotMessage>(this, delta);

			return new WorldSnapshotMessage
			{
				PartyID = deltas.Consume(x => x.PartyID, x => x.ToEqualityDelta(), x => x),
				Secret = deltas.Consume(x => x.Secret, x => x.ToEqualityDelta(), x => x),
				MaxPlayers = deltas.Consume(x => x.MaxPlayers, x => x.ToEqualityDelta(), x => x),

				Level = deltas.Consume(x => x.Level, x => x.ToEqualityDelta(), x => x),

				PlayersLeft = deltas.Consume(x => x.PlayersLeft.ToDeltaBinary()),
				PlayersJoined = deltas.Consume(x => x.PlayersJoined.ToDeltaBinary()),

				Puppets = deltas.Consume(x => x.Puppets.ToDeltaBinary())
			};
		}

		public bool Equals(WorldSnapshotMessage other)
		{
			return PartyID.Equals(other.PartyID) && Secret.Equals(other.Secret) && MaxPlayers.Equals(other.MaxPlayers) &&
				Level.Equals(other.Level, (x, y) => x == y) &&
				PlayersLeft.Equals(other.PlayersLeft, (x, y) => x.SequenceEqual(y)) &&
				PlayersJoined.Equals(other.PlayersJoined, (x, y) => x.SequenceEqual(y)) &&
				Puppets.Equals(other.Puppets, (x, y) => x.SequenceEqual(y));
		}
	}
}
