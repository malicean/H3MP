using H3MP.Differentiation;
using H3MP.Fitting;
using H3MP.IO;
using H3MP.Models;
using H3MP.Serialization;
using H3MP.Utils;
using System.Linq;

namespace H3MP.Messages
{
	public struct WorldSnapshotMessage : ICopyable<WorldSnapshotMessage>
	{
		public byte PlayerID;

		public Key32 PartyID;
		public JoinSecret Secret;

		public string Level;

		public Option<BodyMessage>[] PlayerBodies;

		public WorldSnapshotMessage Copy()
		{
			var playerBodies = new Option<BodyMessage>[PlayerBodies.Length];
			PlayerBodies.CopyTo(playerBodies, 0);

			return new WorldSnapshotMessage
			{
				PlayerID = PlayerID,
				PartyID = PartyID,
				Secret = Secret,

				Level = Level,

				PlayerBodies = playerBodies
			};
		}
	}

	public struct DeltaWorldSnapshotMessage : IOptionComposite
	{
		public Option<byte> PlayerID;
		public Option<Key32> PartyID;
		public Option<JoinSecret> Secret;

		public Option<string> Level;

		public Option<Option<Option<DeltaBodyMessage>>[]> PlayerBodies;

		public bool HasSome => PartyID.IsSome || Secret.IsSome || Level.IsSome || PlayerBodies.IsSome;
	}

	public class WorldSnapshotMessageFitter : IFitter<WorldSnapshotMessage>
	{
		private readonly IFitter<byte> _playerID;
		private readonly IFitter<Key32> _partyID;
		private readonly IFitter<JoinSecret> _secret;

		private readonly IFitter<string> _level;

		private readonly IFitter<Option<BodyMessage>[]> _playerBodies;

		public WorldSnapshotMessageFitter()
		{
			_playerID = Fitters.Oldest<byte>();
			_partyID = Fitters.Oldest<Key32>();
			_secret = Fitters.Oldest<JoinSecret>();

			_level = Fitters.Oldest<string>();

			_playerBodies = new BodyMessageFitter().ToOption().ToFixedArray();
		}

		public WorldSnapshotMessage Fit(WorldSnapshotMessage a, WorldSnapshotMessage b, float t)
		{
			var fitter = new SuperFitter<WorldSnapshotMessage>(a, b, t);

			fitter.Include(x => x.PlayerID, (ref WorldSnapshotMessage body, byte value) => body.PlayerID = value, _playerID);
			fitter.Include(x => x.PartyID, (ref WorldSnapshotMessage body, Key32 value) => body.PartyID = value, _partyID);
			fitter.Include(x => x.Secret, (ref WorldSnapshotMessage body, JoinSecret value) => body.Secret = value, _secret);

			fitter.Include(x => x.Level, (ref WorldSnapshotMessage body, string value) => body.Level = value, _level);

			fitter.Include(x => x.PlayerBodies, (ref WorldSnapshotMessage body, Option<BodyMessage>[] value) => body.PlayerBodies = value, _playerBodies);

			return fitter.Body;
		}
	}

	public class WorldSnapshotMessageDifferentiator : IDifferentiator<WorldSnapshotMessage, DeltaWorldSnapshotMessage>
	{
		private readonly IDifferentiator<byte, byte> _playerID;
		private readonly IDifferentiator<Key32, Key32> _partyID;
		private readonly IDifferentiator<JoinSecret, JoinSecret> _secret;

		private readonly IDifferentiator<string, string> _level;

		private readonly IDifferentiator<Option<BodyMessage>[], Option<Option<DeltaBodyMessage>>[]> _playerBodies;

		public WorldSnapshotMessageDifferentiator()
		{
			_playerID = Differentiators.Equality<byte>();
			_partyID = Differentiators.Equality<Key32>();
			_secret = Differentiators.Equality<JoinSecret>();

			_level = Differentiators.Equality<string>();

			_playerBodies = new BodyMessageDifferentiator(Differentiators.Equality<TransformMessage>()).ToOption().ToArray();
		}

		public WorldSnapshotMessage ConsumeDelta(DeltaWorldSnapshotMessage delta, Option<WorldSnapshotMessage> now)
		{
			var consumer = new SuperDeltaConsumer<DeltaWorldSnapshotMessage, WorldSnapshotMessage>(delta, now);

			consumer.Include(x => x.PlayerID, x => x.PlayerID, (ref WorldSnapshotMessage body, byte value) => body.PlayerID = value, _playerID);
			consumer.Include(x => x.PartyID, x => x.PartyID, (ref WorldSnapshotMessage body, Key32 value) => body.PartyID = value, _partyID);
			consumer.Include(x => x.Secret, x => x.Secret, (ref WorldSnapshotMessage body, JoinSecret value) => body.Secret = value, _secret);

			consumer.Include(x => x.Level, x => x.Level, (ref WorldSnapshotMessage body, string value) => body.Level = value, _level);

			consumer.Include(x => x.PlayerBodies, x => x.PlayerBodies, (ref WorldSnapshotMessage body, Option<BodyMessage>[] value) => body.PlayerBodies = value, _playerBodies);

			return consumer.Body;
		}

		public Option<DeltaWorldSnapshotMessage> CreateDelta(WorldSnapshotMessage now, Option<WorldSnapshotMessage> baseline)
		{
			var creator = new SuperDeltaCreator<WorldSnapshotMessage, DeltaWorldSnapshotMessage>(now, baseline);

			creator.Include(x => x.PlayerID, (ref DeltaWorldSnapshotMessage body, Option<byte> value) => body.PlayerID = value, _playerID);
			creator.Include(x => x.PartyID, (ref DeltaWorldSnapshotMessage body, Option<Key32> value) => body.PartyID = value, _partyID);
			creator.Include(x => x.Secret, (ref DeltaWorldSnapshotMessage body, Option<JoinSecret> value) => body.Secret = value, _secret);

			creator.Include(x => x.Level, (ref DeltaWorldSnapshotMessage body, Option<string> value) => body.Level = value, _level);

			creator.Include(x => x.PlayerBodies, (ref DeltaWorldSnapshotMessage body, Option<Option<Option<DeltaBodyMessage>>[]> value) => body.PlayerBodies = value, _playerBodies);

			return creator.Body;
		}
	}

	public class DeltaWorldSnapshotSerializer : ISerializer<DeltaWorldSnapshotMessage>
	{
		private readonly ISerializer<Option<byte>> _playerID;
		private readonly ISerializer<Option<Key32>> _partyID;
		private readonly ISerializer<Option<JoinSecret>> _secret;

		private readonly ISerializer<Option<string>> _level;

		private readonly ISerializer<Option<Option<Option<DeltaBodyMessage>>[]>> _playerBodies;

		public DeltaWorldSnapshotSerializer(int maxPlayers)
        {
			_playerID = PrimitiveSerializers.Byte.ToOption();
			_partyID = CustomSerializers.Key32.ToOption();
			_secret = CustomSerializers.JoinSecret.ToOption();

			_level = PrimitiveSerializers.Char.ToString(TruncatedSerializers.ByteAsInt).ToOption();

			_playerBodies = new DeltaBodyMessageSerializer().ToOption().ToOption().ToArrayFixed(maxPlayers).ToOption();
        }

		public DeltaWorldSnapshotMessage Deserialize(ref BitPackReader reader)
		{
			return new DeltaWorldSnapshotMessage
			{
				PlayerID = _playerID.Deserialize(ref reader),
				PartyID = _partyID.Deserialize(ref reader),
				Secret = _secret.Deserialize(ref reader),

				Level = _level.Deserialize(ref reader),

				PlayerBodies = _playerBodies.Deserialize(ref reader)
			};
		}

		public void Serialize(ref BitPackWriter writer, DeltaWorldSnapshotMessage value)
		{
			_playerID.Serialize(ref writer, value.PlayerID);
			_partyID.Serialize(ref writer, value.PartyID);
			_secret.Serialize(ref writer, value.Secret);

			_level.Serialize(ref writer, value.Level);

			_playerBodies.Serialize(ref writer, value.PlayerBodies);
		}
	}
}
