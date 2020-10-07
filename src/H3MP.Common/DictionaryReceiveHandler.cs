using System;
using System.Collections.Generic;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Common
{
	public class DictionaryReceiveHandler<TKey>
	{
		public delegate bool FallbackReceiveHandler(NetPeer peer, NetDataReader reader, TKey key);

		private readonly Func<NetDataReader, TKey> _readKey;

		public ReceiveHandler RootHandler { get; }

		public Dictionary<TKey, ReceiveHandler> Handlers { get; set; }

		public FallbackReceiveHandler OnFallback { get; set; }

		public DictionaryReceiveHandler(Func<NetDataReader, TKey> readEnum)
		{
			_readKey = readEnum;

			RootHandler = Handler;
		}

		private void Handler(NetPeer peer, NetDataReader reader)
		{
			var key = _readKey(reader);

			if (!Handlers.TryGetValue(key, out var handler))
			{
				OnFallback?.Invoke(peer, reader, key);
			}
			else
			{
				handler(peer, reader);
			}
		}
	}
}
