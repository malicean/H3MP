using System;
using H3MP.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public struct NetOptionWriter : IDisposable
	{
		private readonly NetDataWriter _writer;
		private readonly NetDataWriter _buffer;
		private readonly IDisposable _bufferHandle;

		private byte _bits;
		private int _index;

		public NetOptionWriter(NetDataWriter writer)
		{
			_writer = writer;
			_bufferHandle = WriterPool.Instance.Borrow(out _buffer);

			_bits = 0;
			_index = 0;
		}

		private void Flush()
		{
			_writer.Put(_bits);
			_writer.Put(_buffer.Data, 0, _buffer.Length);
			_buffer.Reset();
		}

        public void Put<T>(Option<T> option) where T : INetSerializable
		{
			Put<T>(option, (w, v) => w.Put(v));
		}

		public void Put<T>(Option<T> option, Action<NetDataWriter, T> map)
		{
			if (option.MatchSome(out var value))
			{
				return;
			}

			// flush bit buffer
			if (_index >= 8)
			{
				Flush();

				_index = 0;
				_bits = 0;
			}

			// dirty bit
			var index = _index++;
			_bits |= (byte) (1 << index);

			// write
			map(_buffer, value);
		}

		public void Dispose()
        {
			if (_index > 0)
			{
				Flush();
			}
        }
	}
}
