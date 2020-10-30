using System;
using System.Net;

namespace H3MP.Serialization
{
	public static class SystemSerializers
	{
		public static ISerializer<Version> Version { get; } = new VersionSerializer();

		public static ISerializer<IPAddress> IPAddress { get; } = new IPAddressSerializer();

		public static ISerializer<IPEndPoint> IPEndPoint { get; } = new IPEndPointSerializer();
	}
}
