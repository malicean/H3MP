using System;
using System.Linq;
using System.Net;

namespace H3MP.Models
{
	public readonly struct JoinSecret : IEquatable<JoinSecret>
	{
		public readonly Version Version;

		public readonly IPEndPoint EndPoint;

		public readonly Key32 Key;

		public readonly double TickStep;

		public readonly byte MaxPlayers;

		public JoinSecret(Version version, IPEndPoint endPoint, Key32 key, double tickStep, byte maxPlayers)
		{
			Version = version;
			EndPoint = endPoint;
			Key = key;
			TickStep = tickStep;
			MaxPlayers = maxPlayers;
		}

		public bool Equals(JoinSecret other)
		{
			return
				Version == other.Version &&
				EndPoint.Port == other.EndPoint.Port && EndPoint.Address.GetAddressBytes().SequenceEqual(other.EndPoint.Address.GetAddressBytes()) &&
				Key == other.Key &&
				TickStep == other.TickStep &&
				MaxPlayers == other.MaxPlayers;
		}
	}
}
