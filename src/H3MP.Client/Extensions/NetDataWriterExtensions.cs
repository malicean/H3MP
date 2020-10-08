using H3MP.Utils;
using LiteNetLib.Utils;

namespace H3MP
{
	public static class NetDataWriterExtensions
	{
		internal static void Put(this NetDataWriter @this, JoinError value)
		{
			@this.Put((byte) value);
		}

		internal static void Put(this NetDataWriter @this, ConnectionKey value)
		{
			@this.Put(value.Data);
		}
	}
}
