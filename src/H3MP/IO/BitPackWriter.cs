using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.IO
{
	public struct BitPackWriter : IDisposable
	{
		public delegate void Converter<in T>(ref BitPackWriter writer, T value);

		private readonly NetDataWriter _writer;

		private bool _disposed;

		public BitBuffer Bits;
		public Buffer<byte> Bytes;

		public BitPackWriter(NetDataWriter writer)
		{
			_writer = writer;
			_disposed = false;

			Bits = new BitBuffer(32);
			Bytes = new Buffer<byte>(1200);
		}

		public void Dispose()
        {
			if (_disposed)
			{
				return;
			}

			BitBuffer.Serialize(_writer, Bits);

			var bytes = Bytes.Populated;
			_writer.Put(bytes.Array, bytes.Offset, bytes.Count);

			_disposed = true;
        }
	}
}
