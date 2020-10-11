using System;
using Discord;
using H3MP.Networking;
using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP
{
	public static class NetDataReaderExtensions
	{
		internal static JoinError GetJoinError(this NetDataReader @this)
		{
			return (JoinError) @this.GetByte();
		}

		public static Key32 GetKey32(this NetDataReader @this)
		{
			var data = new byte[Key32.SIZE];
			@this.GetBytes(data, Key32.SIZE);

			if (!Key32.TryFromBytes(data, out var value))
			{
				throw new FormatException(nameof(Key32.TryFromBytes) + " returned false (should never happen; data buffer is fixed size).");
			}

			return value;
		}

		public static JoinSecret GetJoinSecret(this NetDataReader @this)
		{
			return new JoinSecret(@this.GetVersion(), @this.GetIPEndPoint(), @this.GetKey32());
		}

		public static PartySize GetPartySize(this NetDataReader @this)
		{
			return new PartySize
			{
				CurrentSize = @this.GetByte(),
				MaxSize = @this.GetByte()
			};
		}
	}
}
