using H3MP.Utils;
using LiteNetLib.Utils;
using System;

namespace H3MP.Networking
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
