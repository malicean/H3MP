using System;
using H3MP.Extensions;
using H3MP.Models;
using LiteNetLib.Utils;

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

		public void Dispose()
        {
			_main.Put(Bits);
			_main.Put(Bytes);

			_bytesHandle.Dispose();
        }
	}
}
