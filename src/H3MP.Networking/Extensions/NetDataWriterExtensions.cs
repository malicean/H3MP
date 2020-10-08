using LiteNetLib.Utils;
using System;

namespace H3MP.Networking
{
	public static class NetDataWriterExtensions
	{
		internal static void Put(this NetDataWriter @this, ConnectionError value)
		{
			@this.Put((byte) value);
		}

		public static void Put(this NetDataWriter @this, Version value)
		{
			@this.Put(value.Major);
			@this.Put(value.Minor);
			@this.Put(value.Build);
			@this.Put(value.Revision);
		}
	}
}
