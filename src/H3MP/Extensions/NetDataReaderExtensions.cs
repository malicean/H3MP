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

		public static ConnectionKey GetConnectionKey(this NetDataReader @this)
		{
			var data = new byte[ConnectionKey.SIZE];
			@this.GetBytes(data, ConnectionKey.SIZE);

			return ConnectionKey.FromBytes(data);
		}
	}
}
