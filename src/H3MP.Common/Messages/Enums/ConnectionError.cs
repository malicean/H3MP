using H3MP.Common.Messages;

using LiteNetLib.Utils;

namespace H3MP.Common.Messages
{
    public enum ConnectionError : byte
    {
        InternalError,
        Closed,
        Full,
		MalformedRequest,
		MismatchedVersion,
        MismatchedPassphrase
    }
}

namespace H3MP.Common.Extensions
{
	public static partial class NetDataWriterExtensions
	{
		public static void Put(this NetDataWriter @this, ConnectionError value)
		{
			@this.Put((byte) value);
		}
	}

	public static partial class NetDataReaderExtensions
	{
		public static ConnectionError GetConnectionError(this NetDataReader @this)
		{
			return (ConnectionError) @this.GetByte();
		}
	}
}
