using H3MP.Common.Messages;

using LiteNetLib.Utils;

namespace H3MP.Server.Extensions
{
	public static partial class NetDataReaderExtensions
	{
		public static ClientMessageType GetMessageType(this NetDataReader @this)
		{
			return (ClientMessageType) @this.GetByte();
		}
	}
}
