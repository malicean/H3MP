using System;
using H3MP.Extensions;
using H3MP.Models;
using H3MP.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public struct BitPackReader
	{
		public delegate T Converter<out T>(ref BitPackReader reader);

		public BitQueue Bits;
		public readonly NetDataReader Bytes;

		public BitPackReader(NetDataReader reader)
		{
			Bits = new BitQueue(reader.GetBitArray());
			Bytes = reader;
		}

		public Option<T> GetOption<T>() where T : IPackedSerializable, new()
		{
			return GetOption<T>((ref BitPackReader reader) =>
			{
				var value = new T();
				value.Deserialize(reader);

				return value;
			});
		}

		public Option<T> GetOption<T>(Converter<T> converter)
		{
			return Bits.Dequeue()
				? Option.Some(converter(ref this))
				: Option.None<T>();
		}
	}
}
