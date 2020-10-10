using H3MP.Utils;
using H3MP.Networking;
using LiteNetLib.Utils;

namespace H3MP
{
	public static class NetDataWriterExtensions
	{
		internal static void Put(this NetDataWriter @this, JoinError value)
		{
			@this.Put((byte) value);
		}

		internal static void Put(this NetDataWriter @this, Key32 value)
		{
			@this.Put(value.Data);
		}

		internal static void Put(this NetDataWriter @this, JoinSecret secret)
		{
			@this.Put(secret.Version);
			@this.Put(secret.EndPoint);
			@this.Put(secret.Key);
		}
	}
}
