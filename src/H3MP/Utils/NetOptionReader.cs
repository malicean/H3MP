using System;
using LiteNetLib.Utils;
using UnityEngine;

namespace H3MP.Utils
{
	public struct NetOptionReader
	{
		private readonly NetDataReader _reader;

		private byte _bits;
		private int _index;

		public NetOptionReader(NetDataReader reader)
		{
			_reader = reader;

			_bits = reader.GetByte();
			_index = 0;
		}

		public Option<T> Get<T>() where T : INetSerializable, new()
		{
			return Get(r => r.Get<T>());
		}

		public Option<T> Get<T>(Func<NetDataReader, T> map)
		{
			// reasses bit buffer
			if (_index >= 8)
			{
				_index = 0;
				_bits = _reader.GetByte();
			}

			// check dirty bit
			var index = _index++;
			var flag = (byte) (1 << index);
			var dirty = (_bits & flag) == flag;

			// read
			return dirty ? Option.Some(map(_reader)) : Option.None<T>();
		}
	}
}
