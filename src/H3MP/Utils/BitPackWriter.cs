using System;
using H3MP.Extensions;
using H3MP.Models;
using H3MP.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public struct BitPackWriter : IDisposable
	{
		public delegate void Converter<in T>(ref BitPackWriter writer, T value);

		private readonly NetDataWriter _main;
		private readonly IDisposable _bytesHandle;

		public BitStack Bits;
		public readonly NetDataWriter Bytes;

		public BitPackWriter(NetDataWriter writer)
		{
			_main = writer;

			Bits = BitStack.CreateDefault();
			_bytesHandle = WriterPool.Instance.Borrow(out Bytes);
		}

		public void Put<T>(T value) where T : IPackedSerializable
		{
			value.Serialize(ref this);
		}

		public void Put<T>(Option<T> value) where T : IPackedSerializable
		{
			Put(value, (ref BitPackWriter w, T v) => v.Serialize(ref w));
		}

		public void Put<T>(Option<T> value, Converter<T> converter)
		{
			if (value.MatchSome(out var inner))
			{
				Bits.Push(true);
				converter(ref this, inner);
			}
			else
			{
				Bits.Push(false);
			}
		}

		public void Dispose()
        {
			_main.Put(Bits);
			_main.Put(Bytes);

			_bytesHandle.Dispose();
        }
	}
}
