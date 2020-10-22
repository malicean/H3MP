using Discord;
using H3MP.Messages;
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
			H3MP.Networking.Extensions.NetDataWriterExtensions.Put(@this, value.EndPoint);
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

		public static void PutBytesWithByteLength(this NetDataWriter @this, byte[] value)
		{
			const byte max = byte.MaxValue;

			var length = value.Length;
			if (length > max)
			{
				throw new ArgumentOutOfRangeException(nameof(value), length, "Length of value must be " + max + " or less.");
			}

			@this.Put((byte) length);
			@this.Put(value);
		}

		public static void PutStringWithByteLength(this NetDataWriter @this, string value)
		{
			const byte max = byte.MaxValue;

			var data = Encoding.UTF8.GetBytes(value);
			var length = data.Length;
			if (length > max)
			{
				throw new ArgumentOutOfRangeException(nameof(value), length, "UTF-8 encoded length of value must be " + max + " or less.");
			}

			@this.PutBytesWithByteLength(data);
		}

		// !!WARNING!!
		// For your own optical health, do not read the following code.
		#region PutDirties Signatures

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3,
			TDirty4 dirty4, Action<NetDataWriter, TValue4> put4
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
			where TDirty4 : IDirty<TValue4> where TValue4 : IEquatable<TValue4>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;
			var dirty4IsDirty = dirty4.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}
			if (dirty4IsDirty)
			{
				bits |= 1 << 3;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}
			if (dirty4IsDirty)
			{
				put4(@this, dirty4.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3,
			TDirty4 dirty4, Action<NetDataWriter, TValue4> put4,
			TDirty5 dirty5, Action<NetDataWriter, TValue5> put5
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
			where TDirty4 : IDirty<TValue4> where TValue4 : IEquatable<TValue4>
			where TDirty5 : IDirty<TValue5> where TValue5 : IEquatable<TValue5>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;
			var dirty4IsDirty = dirty4.IsDirty;
			var dirty5IsDirty = dirty5.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}
			if (dirty4IsDirty)
			{
				bits |= 1 << 3;
			}
			if (dirty5IsDirty)
			{
				bits |= 1 << 4;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}
			if (dirty4IsDirty)
			{
				put4(@this, dirty4.Dirty);
			}
			if (dirty5IsDirty)
			{
				put5(@this, dirty5.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3,
			TDirty4 dirty4, Action<NetDataWriter, TValue4> put4,
			TDirty5 dirty5, Action<NetDataWriter, TValue5> put5,
			TDirty6 dirty6, Action<NetDataWriter, TValue6> put6
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
			where TDirty4 : IDirty<TValue4> where TValue4 : IEquatable<TValue4>
			where TDirty5 : IDirty<TValue5> where TValue5 : IEquatable<TValue5>
			where TDirty6 : IDirty<TValue6> where TValue6 : IEquatable<TValue6>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;
			var dirty4IsDirty = dirty4.IsDirty;
			var dirty5IsDirty = dirty5.IsDirty;
			var dirty6IsDirty = dirty6.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}
			if (dirty4IsDirty)
			{
				bits |= 1 << 3;
			}
			if (dirty5IsDirty)
			{
				bits |= 1 << 4;
			}
			if (dirty6IsDirty)
			{
				bits |= 1 << 5;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}
			if (dirty4IsDirty)
			{
				put4(@this, dirty4.Dirty);
			}
			if (dirty5IsDirty)
			{
				put5(@this, dirty5.Dirty);
			}
			if (dirty6IsDirty)
			{
				put6(@this, dirty6.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
			dirty6.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6,
			TDirty7, TValue7
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3,
			TDirty4 dirty4, Action<NetDataWriter, TValue4> put4,
			TDirty5 dirty5, Action<NetDataWriter, TValue5> put5,
			TDirty6 dirty6, Action<NetDataWriter, TValue6> put6,
			TDirty7 dirty7, Action<NetDataWriter, TValue7> put7
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
			where TDirty4 : IDirty<TValue4> where TValue4 : IEquatable<TValue4>
			where TDirty5 : IDirty<TValue5> where TValue5 : IEquatable<TValue5>
			where TDirty6 : IDirty<TValue6> where TValue6 : IEquatable<TValue6>
			where TDirty7 : IDirty<TValue7> where TValue7 : IEquatable<TValue7>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;
			var dirty4IsDirty = dirty4.IsDirty;
			var dirty5IsDirty = dirty5.IsDirty;
			var dirty6IsDirty = dirty6.IsDirty;
			var dirty7IsDirty = dirty7.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}
			if (dirty4IsDirty)
			{
				bits |= 1 << 3;
			}
			if (dirty5IsDirty)
			{
				bits |= 1 << 4;
			}
			if (dirty6IsDirty)
			{
				bits |= 1 << 5;
			}
			if (dirty7IsDirty)
			{
				bits |= 1 << 6;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}
			if (dirty4IsDirty)
			{
				put4(@this, dirty4.Dirty);
			}
			if (dirty5IsDirty)
			{
				put5(@this, dirty5.Dirty);
			}
			if (dirty6IsDirty)
			{
				put6(@this, dirty6.Dirty);
			}
			if (dirty7IsDirty)
			{
				put7(@this, dirty7.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
			dirty6.Commit();
			dirty7.Commit();
		}

		public static void PutDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6,
			TDirty7, TValue7,
			TDirty8, TValue8
		>(this NetDataWriter @this,
			TDirty1 dirty1, Action<NetDataWriter, TValue1> put1,
			TDirty2 dirty2, Action<NetDataWriter, TValue2> put2,
			TDirty3 dirty3, Action<NetDataWriter, TValue3> put3,
			TDirty4 dirty4, Action<NetDataWriter, TValue4> put4,
			TDirty5 dirty5, Action<NetDataWriter, TValue5> put5,
			TDirty6 dirty6, Action<NetDataWriter, TValue6> put6,
			TDirty7 dirty7, Action<NetDataWriter, TValue7> put7,
			TDirty8 dirty8, Action<NetDataWriter, TValue8> put8
		)
			where TDirty1 : IDirty<TValue1> where TValue1 : IEquatable<TValue1>
			where TDirty2 : IDirty<TValue2> where TValue2 : IEquatable<TValue2>
			where TDirty3 : IDirty<TValue3> where TValue3 : IEquatable<TValue3>
			where TDirty4 : IDirty<TValue4> where TValue4 : IEquatable<TValue4>
			where TDirty5 : IDirty<TValue5> where TValue5 : IEquatable<TValue5>
			where TDirty6 : IDirty<TValue6> where TValue6 : IEquatable<TValue6>
			where TDirty7 : IDirty<TValue7> where TValue7 : IEquatable<TValue7>
			where TDirty8 : IDirty<TValue8> where TValue8 : IEquatable<TValue8>
		{
			byte bits = 0;

			var dirty1IsDirty = dirty1.IsDirty;
			var dirty2IsDirty = dirty2.IsDirty;
			var dirty3IsDirty = dirty3.IsDirty;
			var dirty4IsDirty = dirty4.IsDirty;
			var dirty5IsDirty = dirty5.IsDirty;
			var dirty6IsDirty = dirty6.IsDirty;
			var dirty7IsDirty = dirty7.IsDirty;
			var dirty8IsDirty = dirty8.IsDirty;

			if (dirty1IsDirty)
			{
				bits |= 1 << 0;
			}
			if (dirty2IsDirty)
			{
				bits |= 1 << 1;
			}
			if (dirty3IsDirty)
			{
				bits |= 1 << 2;
			}
			if (dirty4IsDirty)
			{
				bits |= 1 << 3;
			}
			if (dirty5IsDirty)
			{
				bits |= 1 << 4;
			}
			if (dirty6IsDirty)
			{
				bits |= 1 << 5;
			}
			if (dirty7IsDirty)
			{
				bits |= 1 << 6;
			}
			if (dirty8IsDirty)
			{
				bits |= 1 << 7;
			}

			if (dirty1IsDirty)
			{
				put1(@this, dirty1.Dirty);
			}
			if (dirty2IsDirty)
			{
				put2(@this, dirty2.Dirty);
			}
			if (dirty3IsDirty)
			{
				put3(@this, dirty3.Dirty);
			}
			if (dirty4IsDirty)
			{
				put4(@this, dirty4.Dirty);
			}
			if (dirty5IsDirty)
			{
				put5(@this, dirty5.Dirty);
			}
			if (dirty6IsDirty)
			{
				put6(@this, dirty6.Dirty);
			}
			if (dirty7IsDirty)
			{
				put7(@this, dirty7.Dirty);
			}
			if (dirty8IsDirty)
			{
				put8(@this, dirty8.Dirty);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
			dirty6.Commit();
			dirty7.Commit();
			dirty8.Commit();
		}

		#endregion
	}
}
