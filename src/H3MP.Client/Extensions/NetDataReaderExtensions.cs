using H3MP.Common.Messages;

using LiteNetLib.Utils;

namespace H3MP.Client.Extensions
{
	public static partial class NetDataReaderExtensions
	{
		public static ServerMessageType GetMessageType(this NetDataReader @this)
		{
			return (ServerMessageType) @this.GetByte();
		}
	}
}
