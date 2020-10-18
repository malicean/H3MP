using System;
using System.Net;
using System.Security.Cryptography;
using H3MP.Networking;
using LiteNetLib.Utils;

namespace H3MP.Models
{
	public readonly struct JoinSecret
	{
		public Version Version { get; }

		public IPEndPoint EndPoint { get; }

		public Key32 Key { get; }

		public byte UpdatesPerTick { get; }

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
			byte updatesPerTick;
			try
			{
				endPoint = reader.GetIPEndPoint();
				key = reader.GetKey32();
				updatesPerTick = reader.GetByte();
			}
			catch
			{
				result = default;
				return false;
			}

			result = new JoinSecret(version, endPoint, key, updatesPerTick);
			return true;
		}

		public JoinSecret(Version version, IPEndPoint endPoint, Key32 key, byte updatesPerTick)
		{
			Version = version;
			EndPoint = endPoint;
			Key = key;
			UpdatesPerTick = updatesPerTick;
		}

		public string ToBase64()
		{
			using (WriterPool.Instance.Borrow(out var writer))
			{
				writer.Put(Version);
				writer.Put(EndPoint);
				writer.Put(Key);
				writer.Put(UpdatesPerTick);

				return Convert.ToBase64String(writer.Data, 0, writer.Length);
			}
		}
	
	}
}
