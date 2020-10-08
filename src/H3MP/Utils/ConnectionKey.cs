using System;
using System.Security.Cryptography;

namespace H3MP.Utils
{
	public readonly struct ConnectionKey : IEquatable<ConnectionKey>
	{
		public const int SIZE = 128;

		public static ConnectionKey FromRandom(RandomNumberGenerator rng)
		{
			var data = new byte[SIZE];
			rng.GetBytes(data);

			return new ConnectionKey(data);
		}

		public static ConnectionKey FromBytes(byte[] data)
		{
			if (data.Length != SIZE)
			{
				throw new ArgumentOutOfRangeException(nameof(data), data.Length, "Data length must match key size (" + SIZE + ").");
			}

			return new ConnectionKey(data);
		}

		public static ConnectionKey FromString(string b64data)
		{
			var data = Convert.FromBase64String(b64data);

			return FromBytes(data);
		}

		public byte[] Data { get; }

		private ConnectionKey(byte[] data)
		{
			Data = data;
		}

		public bool Equals(ConnectionKey other)
		{
			if (Data.Length != other.Data.Length)
			{
				return false;
			}

			for (var i = 0; i < Data.Length; ++i)
			{
				if (Data[i] != other.Data[i])
				{
					return false;
				}
			}

			return true;
		}

		public override string ToString()
		{
			return Convert.ToBase64String(Data);
		}

		public static bool operator ==(ConnectionKey key1, ConnectionKey key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(ConnectionKey key1, ConnectionKey key2)
		{
			return !(key1 == key2);
		}
	}
}
