using H3MP.Common.Messages;

using LiteNetLib.Utils;

namespace H3MP.Server.Extensions
{
	public static partial class NetDataWriterExtensions
	{
		public static void Put(this NetDataWriter @this, ServerMessageType value)
		{
			@this.Put((byte) value);
		}

		public static void PutTyped(this NetDataWriter @this, PongMessage value)
		{
			@this.Put(ServerMessageType.Pong);
			@this.Put(value);
		}

		public static void PutTyped(this NetDataWriter @this, LevelChangeMessage value)
		{
			@this.Put(ServerMessageType.LevelChange);
			@this.Put(value);
		}
	}
}
