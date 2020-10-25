using H3MP.Extensions;
using H3MP.Networking;
using H3MP.Networking.Extensions;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Security.Cryptography;

namespace H3MP.Models
{
	public readonly struct JoinSecret : IEquatable<JoinSecret>
	{
		public Version Version { get; }

		public IPEndPoint EndPoint { get; }

		public Key32 Key { get; }

		public double TickDeltaTime { get; }

		public static bool TryParse(byte[] data, out JoinSecret result, out Version version)
		{
			var reader = new NetDataReader(data);

			try
			{
				version = reader.GetVersion();
			}
			catch
			{
				result = default;
				version = default;
				return false;
			}

			IPEndPoint endPoint;
			Key32 key;
			double tickDeltaTime;
			try
			{
				endPoint = reader.GetIPEndPoint();
				key = reader.GetKey32();
				tickDeltaTime = reader.GetDouble();
			}
			catch
			{
				result = default;
				return false;
			}

			if (tickDeltaTime <= 0)
			{
				result = default;
				return false;
			}

			result = new JoinSecret(version, endPoint, key, tickDeltaTime);
			if (reader.AvailableBytes > 0)
			{
				return false;
			}

			return true;
		}

		public JoinSecret(Version version, IPEndPoint endPoint, Key32 key, double tickDeltaTime)
		{
			Version = version;
			EndPoint = endPoint;
			Key = key;
			TickDeltaTime = tickDeltaTime;
		}

		public string ToBase64()
		{
			using (WriterPool.Instance.Borrow(out var writer))
			{
				writer.Put(this);

				return Convert.ToBase64String(writer.Data, 0, writer.Length);
			}
		}

		public bool Equals(JoinSecret other)
		{
			return Version == other.Version && EndPoint == other.EndPoint && Key == other.Key && TickDeltaTime == other.TickDeltaTime;
		}
	}
}
