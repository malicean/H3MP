using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public static class NetToPackedSerializableExtensions
	{
		public static NetToPackedSerializable<T> ToPacked<T>(this T @this) where T : INetSerializable
		{
			return new NetToPackedSerializable<T>(@this);
		}
	}

	public struct NetToPackedSerializable<T> : IPackedSerializable, IRef<T> where T : INetSerializable
	{
		private T _value;

		T IRef<T>.Value => _value;

		public NetToPackedSerializable(T value)
		{
			_value = value;
		}

		public void Deserialize(BitPackReader reader)
		{
			_value.Deserialize(reader.Bytes);
		}

		public void Serialize(BitPackWriter writer)
		{
			_value.Serialize(writer.Bytes);
		}

		public static implicit operator T(NetToPackedSerializable<T> @this)
		{
			return @this._value;
		}
	}
}
