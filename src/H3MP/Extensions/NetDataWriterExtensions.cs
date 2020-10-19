using H3MP.Utils;
using H3MP.Networking;
using LiteNetLib.Utils;
using System;
using Discord;
using UnityEngine;
using H3MP.Models;

namespace H3MP
{
	public static class NetDataWriterExtensions
	{
		internal static void Put(this NetDataWriter @this, JoinError value)
		{
			@this.Put((byte) value);
		}

		public static void Put<T>(this NetDataWriter @this, T? value) where T : struct, INetSerializable
		{
			@this.Put(value, (writer, cvalue) => writer.Put(cvalue));
		}

		public static void Put<T>(this NetDataWriter @this, T? value, Action<NetDataWriter, T> writer) where T : struct
		{
			var hasValue = value.HasValue;

			@this.Put(hasValue);
			if (hasValue)
			{
				writer(@this, value.Value);
			}
		}

		public static void Put(this NetDataWriter @this, Key32 value)
		{
			@this.Put(value.Data);
		}

		public static void Put(this NetDataWriter @this, JoinSecret value)
		{
			@this.Put(value.Version);
			H3MP.Networking.NetDataWriterExtensions.Put(@this, value.EndPoint);
			@this.Put(value.Key);
			@this.Put(value.TickDeltaTime);
		}

		public static void Put(this NetDataWriter @this, PartySize value)
		{
			@this.Put((byte) value.CurrentSize);
			@this.Put((byte) value.MaxSize);
		}

		public static void Put(this NetDataWriter @this, Vector3 value)
		{
			@this.Put(value.x);
			@this.Put(value.y);
			@this.Put(value.z);
		}

		public static void Put(this NetDataWriter @this, Quaternion value)
		{
			@this.Put(value.x);
			@this.Put(value.y);
			@this.Put(value.z);
			@this.Put(value.w);
		}
	}
}
