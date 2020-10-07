using H3MP.Common.Extensions;

using System;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Common
{
	public delegate void MessageHandler<in TMessage>(NetPeer peer, TMessage message) where TMessage : INetSerializable, new();

	public static class MessageHandler
	{
		public static ReceiveHandler ToNetHandler<TMessage>(this MessageHandler<TMessage> @this, Action<NetPeer> onFailure) where TMessage : INetSerializable, new()
		{
			return (peer, reader) =>
			{
				if (!reader.TryCatchGet<TMessage>(out var message))
				{
					onFailure(peer);
				}
				else
				{
					@this(peer, message);
				}
			};
		}
	}
}
