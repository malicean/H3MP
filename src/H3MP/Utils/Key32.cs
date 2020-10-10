using System;
using System.Security.Cryptography;

namespace H3MP.Utils
{
	public readonly struct Key32 : IEquatable<Key32>
	{
		public const int SIZE = 32;

		public static Key32 FromRandom(RandomNumberGenerator rng)
		{
			var data = new byte[SIZE];
			rng.GetBytes(data);

			return new Key32(data);
		}

		public static bool TryFromBytes(byte[] data, out Key32 value)
		{
			if (data.Length != SIZE)
			{
				value = default;
				return false;
			}

			value = new Key32(data);
			return true;
		}

		public static bool TryFromString(string b64data, out Key32 value)
		{
			var data = Convert.FromBase64String(b64data);

			return TryFromBytes(data, out value);
		}

		public byte[] Data { get; }

		private Key32(byte[] data)
		{
			Data = data;
		}

		public bool Equals(Key32 other)
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

		public static bool operator ==(Key32 key1, Key32 key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(Key32 key1, Key32 key2)
		{
			return !(key1 == key2);
		}
	}
}
