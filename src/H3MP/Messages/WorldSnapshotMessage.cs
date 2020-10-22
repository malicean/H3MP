using H3MP.Extensions;
using H3MP.Models;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace H3MP.Messages
{
    public struct WorldSnapshotMessage : INetSerializable
	{
		[Flags]
		public enum FieldBits : byte
		{
			PartyID = 1 << 0,
			Secret = 1 << 1,
			MaxPlayers = 1 << 2,
			Level = 1 << 3,
			PlayersLeft = 1 << 4,
			PlayersJoined = 1 << 5,
			Puppets = 1 << 6,
		}

		private FieldBits _fields;

		private Key32? _partyID;
		public Key32? PartyID
		{
			get => _partyID;
			set
			{
				SetField(FieldBits.PartyID, value.HasValue);
				_partyID = value;
			}
		}

		private JoinSecret? _secret;
		public JoinSecret? Secret
		{
			get => _secret;
			set
			{
				SetField(FieldBits.Secret, _secret.HasValue);
				_secret = value;
			}
		}

		private byte _maxPlayers;
		public byte MaxPlayers
		{
			get => _maxPlayers;
			set
			{
				SetField(FieldBits.MaxPlayers, _maxPlayers > 0);
				_maxPlayers = value;
			}
		}

		private string _level;
		public string Level
		{
			get => _level;
			set
			{
				SetField(FieldBits.Level, !(value is null));
				_level = value;
			}
		}

		private byte[] _playersLeft;
		public byte[] PlayersLeft
		{
			get => _playersLeft;
			set
			{
				SetField(FieldBits.PlayersLeft, !(value is null));
				_playersLeft = value;
			}
		}

		private byte[] _playersJoined;
		public byte[] PlayersJoined
		{
			get => _playersJoined;
			set
			{
				SetField(FieldBits.PlayersJoined, !(value is null));
				_playersJoined = value;
			}
		}

		private Dictionary<byte, MoveMessage> _puppets;
		public Dictionary<byte, MoveMessage> Puppets
		{
			get => _puppets;
			set
			{
				SetField(FieldBits.Puppets, !(value is null));
				_puppets = value;
			}
		}

		private void SetField(FieldBits field, bool has)
		{
			if (has)
			{
				_fields |= field;
			}
			else
			{
				_fields &= ~field;
			}
		}

		private bool HasField(FieldBits field)
		{
			return (_fields & field) == field;
		}

		public void Deserialize(NetDataReader reader)
		{
			_fields = (FieldBits) reader.GetByte();

			if (HasField(FieldBits.PartyID))
			{
				_partyID = reader.GetKey32();
			}
			if (HasField(FieldBits.Secret))
			{
				_secret = reader.GetJoinSecret();
			}
			if (HasField(FieldBits.MaxPlayers))
			{
				_maxPlayers = reader.GetByte();
			}

			if (HasField(FieldBits.Level))
			{
				_level = reader.GetStringWithByteLength();
			}

			if (HasField(FieldBits.PlayersLeft))
			{
				_playersLeft = reader.GetBytesWithByteLength();
			}
			if (HasField(FieldBits.PlayersJoined))
			{
				_playersJoined = reader.GetBytesWithByteLength();
			}
			if (HasField(FieldBits.Puppets))
			{
				var count = reader.GetByte();
				_puppets = new Dictionary<byte, MoveMessage>(count);

				for (var i = 0; i < count; ++i)
				{
					_puppets.Add(reader.GetByte(), reader.Get<MoveMessage>());
				}
			}
		}

		public void Serialize(NetDataWriter writer)
		{
			writer.Put((byte) _fields);

			if (HasField(FieldBits.PartyID))
			{
				writer.Put(_partyID.Value);
			}
			if (HasField(FieldBits.Secret))
			{
				writer.Put(_secret.Value);
			}
			if (HasField(FieldBits.MaxPlayers))
			{
				writer.Put(_maxPlayers);
			}

			if (HasField(FieldBits.Level))
			{
				writer.PutStringWithByteLength(_level);
			}

			if (HasField(FieldBits.PlayersLeft))
			{
				writer.PutBytesWithByteLength(_playersLeft);
			}
			if (HasField(FieldBits.PlayersJoined))
			{
				writer.PutBytesWithByteLength(_playersJoined);
			}
			if (HasField(FieldBits.Puppets))
			{
				var count = (byte) Puppets.Count;
				writer.Put(count);

				foreach (KeyValuePair<byte, MoveMessage> puppet in _puppets)
				{
					writer.Put(puppet.Key);
					writer.Put(puppet.Value);
				}
			}
		}
	}
}
