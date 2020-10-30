using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Net;

namespace H3MP.Models
{
    public readonly struct JoinSecret
	{
		public Version Version { get; }

		public IPEndPoint EndPoint { get; }

		public Key32 Key { get; }

		public double TickDeltaTime { get; }

		public JoinSecret(Version version, IPEndPoint endPoint, Key32 key, double tickDeltaTime)
		{
			Version = version;
			EndPoint = endPoint;
			Key = key;
			TickDeltaTime = tickDeltaTime;
		}
	}
}
