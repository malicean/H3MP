using Discord;
using H3MP.Models;
using H3MP.Networking;
using H3MP.Networking.Extensions;
using H3MP.Utils;
using LiteNetLib.Utils;
using System;
using System.Text;
using UnityEngine;

namespace H3MP.Extensions
{
	public static class NetDataReaderExtensions
	{
		internal static JoinError GetJoinError(this NetDataReader @this)
		{
			return (JoinError) @this.GetByte();
		}

		public static T? GetNullable<T>(this NetDataReader @this) where T : struct, INetSerializable
		{
			return @this.GetNullable(reader => reader.Get<T>());
		}

		public static T? GetNullable<T>(this NetDataReader @this, Func<NetDataReader, T> reader) where T : struct
		{
			var hasValue = @this.GetBool();
			return hasValue ? reader(@this) : (T?) null;
		}

		public static Key32 GetKey32(this NetDataReader @this)
		{
			var data = new byte[Key32.SIZE];
			@this.GetBytes(data, Key32.SIZE);

			if (!Key32.TryFromBytes(data, out var value))
			{
				throw new FormatException(nameof(Key32.TryFromBytes) + " returned false (should never happen; data buffer is fixed size).");
			}

			return value;
		}

		public static JoinSecret GetJoinSecret(this NetDataReader @this)
		{
			return new JoinSecret(@this.GetVersion(), @this.GetIPEndPoint(), @this.GetKey32(), @this.GetDouble());
		}

		public static PartySize GetPartySize(this NetDataReader @this)
		{
			return new PartySize
			{
				CurrentSize = @this.GetByte(),
				MaxSize = @this.GetByte()
			};
		}

		public static Vector3 GetVector3(this NetDataReader @this)
		{
			return new Vector3(@this.GetFloat(), @this.GetFloat(), @this.GetFloat());
		}

		public static Quaternion GetQuaternion(this NetDataReader @this)
		{
			return new Quaternion(@this.GetFloat(), @this.GetFloat(), @this.GetFloat(), @this.GetFloat());
		}

		public static byte[] GetBytesWithByteLength(this NetDataReader @this)
		{
			var length = @this.GetByte();
			var data = new byte[length];

			@this.GetBytes(data, 0, length);

			return data;
		}

		public static string GetStringWithByteLength(this NetDataReader @this)
		{
			var data = @this.GetBytesWithByteLength();

			return Encoding.UTF8.GetString(data);
		}
	}
}
