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

		// !!WARNING!!
		// For your own optical health, do not read the following code.
		#region GetDirties Signatures

		public static void GetOptions<
			TDirty1, TValue1,
			TDirty2, TValue2
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3,
			TDirty4 dirty4, Func<NetDataReader, TValue4> read4
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
			where TValue4 : IEquatable<TValue4>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}
			if (dirty4IsDirty)
			{
				dirty4.Dirty = read4(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3,
			TDirty4 dirty4, Func<NetDataReader, TValue4> read4,
			TDirty5 dirty5, Func<NetDataReader, TValue5> read5
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
			where TValue4 : IEquatable<TValue4>
			where TValue5 : IEquatable<TValue5>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}
			if (dirty4IsDirty)
			{
				dirty4.Dirty = read4(@this);
			}
			if (dirty5IsDirty)
			{
				dirty5.Dirty = read5(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3,
			TDirty4 dirty4, Func<NetDataReader, TValue4> read4,
			TDirty5 dirty5, Func<NetDataReader, TValue5> read5,
			TDirty6 dirty6, Func<NetDataReader, TValue6> read6
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
			where TValue4 : IEquatable<TValue4>
			where TValue5 : IEquatable<TValue5>
			where TValue6 : IEquatable<TValue6>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}
			if (dirty4IsDirty)
			{
				dirty4.Dirty = read4(@this);
			}
			if (dirty5IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}
			if (dirty6IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
			dirty6.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6,
			TDirty7, TValue7
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3,
			TDirty4 dirty4, Func<NetDataReader, TValue4> read4,
			TDirty5 dirty5, Func<NetDataReader, TValue5> read5,
			TDirty6 dirty6, Func<NetDataReader, TValue6> read6,
			TDirty7 dirty7, Func<NetDataReader, TValue7> read7
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
			where TValue4 : IEquatable<TValue4>
			where TValue5 : IEquatable<TValue5>
			where TValue6 : IEquatable<TValue6>
			where TValue7 : IEquatable<TValue7>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}
			if (dirty4IsDirty)
			{
				dirty4.Dirty = read4(@this);
			}
			if (dirty5IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}
			if (dirty6IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}
			if (dirty7IsDirty)
			{
				dirty7.Dirty = read7(@this);
			}

			dirty1.Commit();
			dirty2.Commit();
			dirty3.Commit();
			dirty4.Commit();
			dirty5.Commit();
			dirty6.Commit();
			dirty7.Commit();
		}

		public static void GetDirties<
			TDirty1, TValue1,
			TDirty2, TValue2,
			TDirty3, TValue3,
			TDirty4, TValue4,
			TDirty5, TValue5,
			TDirty6, TValue6,
			TDirty7, TValue7,
			TDirty8, TValue8
		>(this NetDataReader @this,
			TDirty1 dirty1, Func<NetDataReader, TValue1> read1,
			TDirty2 dirty2, Func<NetDataReader, TValue2> read2,
			TDirty3 dirty3, Func<NetDataReader, TValue3> read3,
			TDirty4 dirty4, Func<NetDataReader, TValue4> read4,
			TDirty5 dirty5, Func<NetDataReader, TValue5> read5,
			TDirty6 dirty6, Func<NetDataReader, TValue6> read6,
			TDirty7 dirty7, Func<NetDataReader, TValue7> read7,
			TDirty8 dirty8, Func<NetDataReader, TValue8> read8
		)
			where TValue1 : IEquatable<TValue1>
			where TValue2 : IEquatable<TValue2>
			where TValue3 : IEquatable<TValue3>
			where TValue4 : IEquatable<TValue4>
			where TValue5 : IEquatable<TValue5>
			where TValue6 : IEquatable<TValue6>
			where TValue7 : IEquatable<TValue7>
			where TValue8 : IEquatable<TValue8>
		{
			byte bits = @this.GetByte();

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
				dirty1.Dirty = read1(@this);
			}
			if (dirty2IsDirty)
			{
				dirty2.Dirty = read2(@this);
			}
			if (dirty3IsDirty)
			{
				dirty3.Dirty = read3(@this);
			}
			if (dirty4IsDirty)
			{
				dirty4.Dirty = read4(@this);
			}
			if (dirty5IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}
			if (dirty6IsDirty)
			{
				dirty6.Dirty = read6(@this);
			}
			if (dirty7IsDirty)
			{
				dirty7.Dirty = read7(@this);
			}
			if (dirty8IsDirty)
			{
				dirty8.Dirty = read8(@this);
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
