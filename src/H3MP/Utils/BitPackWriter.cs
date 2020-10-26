using System;
using H3MP.Extensions;
using H3MP.Models;
using LiteNetLib.Utils;

namespace H3MP.Utils
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

			var bits = Bits.Populated;
			_writer.Put(bits.Array, bits.Offset, bits.Count);

			var bytes = Bytes.Populated;
			_writer.Put(bytes.Array, bits.Offset, bits.Count);

			_disposed = true;
        }
	}
}
