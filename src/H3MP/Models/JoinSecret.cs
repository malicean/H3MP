using System;
using System.Net;
using System.Security.Cryptography;

namespace H3MP.Models
{
	public readonly struct JoinSecret
	{
		public Version Version { get; }

		public IPEndPoint EndPoint { get; }

		public Key32 Key { get; }

		public static bool TryParse(string s, out JoinSecret result, out Version version)
		{
			const int expectedFields = 4;
			var args = s.Split(' ');

			if (args.Length < 1)
			{
				result = default;
				version = default;
				return false;
			}

			var rawVersion = args[0];
			try
			{
				version = new Version(rawVersion);
			}
			catch
			{
				result = default;
				version = default;
				return false;
			}

			if (args.Length != expectedFields)
			{
				result = default;
				return false;
			}

			if (!IPAddress.TryParse(args[1], out var address) || !ushort.TryParse(args[2], out var port) || !Key32.TryFromString(args[3], out var key))
			{
				result = default;
				return false;
			}

			result = new JoinSecret(version, new IPEndPoint(address, port), key);
			return true;
		}

		public JoinSecret(Version version, IPEndPoint endPoint, Key32 key)
		{
			Version = version;
			EndPoint = endPoint;
			Key = key;
		}

		public override string ToString()
		{
			return Version + " " + EndPoint.Address + " " + EndPoint.Port + " " + Key;
		}
	
	}
}
