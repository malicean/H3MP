using LiteNetLib.Utils;
using System;
using System.Net;

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

		public static void Put(this NetDataWriter @this, IPAddress value)
		{
			var bytes = value.GetAddressBytes();

			@this.Put((byte) bytes.Length);
			@this.Put(bytes);
		}

		public static void Put(this NetDataWriter @this, IPEndPoint value)
		{
			@this.Put(value.Address);
			@this.Put((ushort) value.Port);
		}
	}
}
