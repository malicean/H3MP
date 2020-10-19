using LiteNetLib.Utils;
using System;
using System.Net;

namespace H3MP.Networking.Extensions
{
    public static class NetDataReaderExtensions
	{
		internal static ConnectionError GetConnectionError(this NetDataReader @this)
		{
			return (ConnectionError) @this.GetByte();
		}

		public static Version GetVersion(this NetDataReader @this)
		{
			// This is the worst System type.
			// It's not even semver.

			var major = @this.GetInt();
			var minor = @this.GetInt();
			var build = @this.GetInt();
			var revision = @this.GetInt();

			if (build == -1)
			{
				return new Version(major, minor);
			}
			else if (revision == -1)
			{
				return new Version(major, minor, build);
			}
			else
			{
				return new Version(major, minor, build, revision);
			}
		}

		public static IPAddress GetIPAddress(this NetDataReader @this)
		{
			var bytes = new byte[@this.GetByte()];
			@this.GetBytes(bytes, bytes.Length);

			return new IPAddress(bytes);
		}

		public static IPEndPoint GetIPEndPoint(this NetDataReader @this)
		{
			return new IPEndPoint(@this.GetIPAddress(), @this.GetUShort());
		}

		public static bool TryGet<TMessage>(this NetDataReader @this, out TMessage value) where TMessage : INetSerializable, new()
		{
			try
			{
				value = @this.Get<TMessage>();
			}
			catch
			{
				value = default;
				return false;
			}

			return true;
		}
	}
}
