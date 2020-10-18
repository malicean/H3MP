using BepInEx.Logging;
using H3MP.Messages;
using H3MP.Networking;
using H3MP.Utils;

namespace H3MP.Models
{
	// Heavily inspired by Mirror Networking for Unity (I loved using this feature).
	// Sources:
	//	- Mirror (abstract): https://mirror-networking.com/docs/Articles/Guides/ClockSync.html
	//	- Mirror (source): https://github.com/vis2k/Mirror/blob/master/Assets/Mirror/Runtime/NetworkTime.cs
	/// <summary>
	///		Synchronizes the server-localized time in real time on the client.
	/// </summary>
	internal class ServerTime
	{
		/// <summary>
		///		Determines the stability of the offset before it is "squeezed" (technically never, but practically when the range is 2ms).
		///		Range is (0, 1).
		///		When it is >0.5, latest offset matters more.
		///		When it is <0.5, history offset matters more.
		/// </summary>
		private const double OFFSET_EMA_ALPHA = 1d / 5;

		private const double PING_INTERVAL = 3;
		private const double HEALTH_INTERVAL = 60;

		private readonly ManualLogSource _log;
		private readonly Peer _server;

		private readonly LoopTimer _pingTimer;
		private readonly LoopTimer _healthTimer;

		private readonly ExponentialMovingAverage _offsetAverage;
		private readonly ExponentialMovingAverage _rttAverage;

		private DoubleRange _offsetBounds;
		private DoubleRange _rttBounds;

		private byte _sent;
		private byte _received;

		/// <summary>
		///		The <see cref="LocalTime.Now" /> value now, in real time, on the server.
		/// </summary>
		public double Now => LocalTime.Now - _offsetAverage.Value;

		public ServerTime(ManualLogSource log, Peer server, Timestamped<PingMessage> seed) 
		{
			_log = log;
			_server = server;

			_pingTimer = new LoopTimer(PING_INTERVAL);
			_healthTimer = new LoopTimer(HEALTH_INTERVAL);

			var offset = ProcessPong(seed, out var rtt, out _offsetBounds);
			_rttBounds = new DoubleRange(rtt, rtt);

			_offsetAverage = new ExponentialMovingAverage(offset, OFFSET_EMA_ALPHA);
			_rttAverage = new ExponentialMovingAverage(rtt, OFFSET_EMA_ALPHA);
		}

		/// <summary>
		///		Start an offset update if enough time has passed.
		/// </summary>
		/// <param name="server">The server peer.</param>
		public void Update()
		{
			if (_healthTimer.TryReset())
			{
				var lost = _sent - _received;
				var loss = (double) lost / _sent;

				_log.LogDebug("=== CONNECTION HEALTH REPORT ===");
				_log.LogDebug($"RTT: {_rttAverage.Value * 1000:N0}ms");
				_log.LogDebug($"Packet loss: {loss:P1} ({lost} / {_sent})");
				_log.LogDebug($"Clock offset: {_offsetBounds.Minimum:.000}s <= est. {_offsetAverage.Value:.000}s <= {_offsetBounds.Maximum:.000}s");
				_log.LogDebug("=== END HEALTH REPORT ===");

				_sent = 0;
				_received = 0;
			}

			if (_pingTimer.TryReset())
			{
				_server.Send(PingMessage.Now);
				++_sent;
			}
		}

		private double ProcessPong(Timestamped<PingMessage> message, out double rtt, out DoubleRange bounds)
		{
			var now = LocalTime.Now;

			var server = message.Timestamp;
			var client = message.Content.Timestamp;
			rtt = now - client;

			// The difference between the reply time locally (estimated) and remotely (known).
			// Calculated by getting the instant equally between when the message was sent and when it was received.
			var offset = client +
				rtt * 0.5 // estimated time until server response
				- server;

			// Basically squeeze theorem: if we absolutely know the range of the real offset, keep shrinking it until the difference is infinitely small (epsilon).
			bounds = new DoubleRange(client - server, now - server);

			return offset;
		}

		// This is almost a direct rip from Mirror's NetworkTime.OnClientPong(...), but at least I understand how it works.
		/// <summary>
		///		Finishes an offset update.
		/// </summary>
		/// <param name="message">The times sent by the server, including the original send time.</param>
		public void ProcessPong(Timestamped<PingMessage> message)
		{
			++_received;

			var offset = ProcessPong(message, out var rtt, out var offsetBounds);

			_offsetBounds = _offsetBounds.Clamp(offsetBounds);

			if (!_offsetBounds.IsWithin(_offsetAverage.Value)) // Average does not exist or is out of known bounds.
			{
				_offsetAverage.Reset(_offsetBounds.Clamp(offset));
			}
			else if (_offsetBounds.IsWithin(offset)) // New offset is within bounds.
			{
				_offsetAverage.Push(offset);
			}

			_rttAverage.Push(rtt);
		}
	}
}
