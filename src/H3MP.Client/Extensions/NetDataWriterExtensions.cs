using H3MP.Common.Messages;

using LiteNetLib.Utils;

namespace H3MP.Client.Extensions
{
	public static partial class NetDataWriterExtensions
	{
		public static void Put(this NetDataWriter @this, ClientMessageType value)
		{
			@this.Put((byte) value);
		}

		public static void PutTyped(this NetDataWriter @this, PingMessage value)
		{
			@this.Put(ClientMessageType.Ping);
			@this.Put(value);
		}
	}
}
