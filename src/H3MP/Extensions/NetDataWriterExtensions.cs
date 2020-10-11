using H3MP.Utils;
using H3MP.Networking;
using LiteNetLib.Utils;
using Discord;

namespace H3MP
{
	public static class NetDataWriterExtensions
	{
		internal static void Put(this NetDataWriter @this, JoinError value)
		{
			@this.Put((byte) value);
		}

		public static void Put(this NetDataWriter @this, Key32 value)
		{
			@this.Put(value.Data);
		}

		public static void Put(this NetDataWriter @this, JoinSecret value)
		{
			@this.Put(value.Version);
			H3MP.Networking.NetDataWriterExtensions.Put(@this, value.EndPoint);
			@this.Put(value.Key);
		}

		public static void Put(this NetDataWriter @this, PartySize value)
		{
			@this.Put((byte) value.CurrentSize);
			@this.Put((byte) value.MaxSize);
		}
	}
}
