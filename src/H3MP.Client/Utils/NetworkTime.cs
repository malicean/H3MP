using H3MP.Client.Extensions;
using H3MP.Common.Messages;
using H3MP.Common.Utils;

using BepInEx.Logging;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Client.Utils
{
	public class NetworkTime
	{
		// 2/3 seems to be an ok value, maybe change if we want more volatility/stability.
		public const double EMA_ALPHA = 2d / 3;

		private readonly ManualLogSource _logger;
		private readonly Pool<NetDataWriter> _writers;
		private readonly ExponentialMovingAverage _offset;

		private double _lastSend;

		public double Offset { get; private set; }

		public double Now => LocalTime.Now - Offset;

		public NetworkTime(ManualLogSource logger, Pool<NetDataWriter> writers) 
		{
			_logger = logger;
			_writers = writers;
			_offset = new ExponentialMovingAverage(EMA_ALPHA);
		}

		public void StartUpdate(NetPeer peer)
		{
			_writers.Borrow(out var writer);

			_lastSend = LocalTime.Now;
			writer.PutTyped(new PingMessage(_lastSend));

			peer.Send(writer, DeliveryMethod.ReliableSequenced);
		}

		public void FinishUpdate(PongMessage pong)
		{
			// Old ping.
			if (pong.SeedTime < _lastSend)
			{
				return;
			}

			var now = LocalTime.Now;
			// Round trip time
			var rtt = now - pong.SeedTime;

			// The client's (approximated) time at the instant the server responded.
			var localReplyTime = now - rtt * 0.5;
			// The server's time at the instant the server responded.
			var remoteReplyTime = pong.ReplyTime;

			var offset = localReplyTime - remoteReplyTime;
			Offset = _offset.Push(offset);

			_logger.LogDebug($"RTT: {rtt:.000}s; Offset: {Offset:.000}s");
		}
	}
}
