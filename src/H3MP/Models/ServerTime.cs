using BepInEx.Logging;
using H3MP.Messages;
using H3MP.Networking;
using H3MP.Utils;
using System;

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
		public delegate void OnPongReceived(double offset, double rtt);

		/// <summary>
		///		Determines the stability of the offset before it is "squeezed" (technically never, but practically when the range is 2ms).
		///		Range is (0, 1).
		///		When it is >0.5, latest offset matters more.
		///		When it is <0.5, history offset matters more.
		/// </summary>
		private const double EMA_ALPHA = 2d / 10;

		private readonly ManualLogSource _log;
		private readonly Peer _server;

		private readonly LoopTimer _pingTimer;

		private readonly ExponentialMovingAverage _offsetAverage;
		private readonly ExponentialMovingAverage _rttAverage;

		public double Offset => _offsetAverage.Value;

		private DoubleRange _offsetBounds;
		public DoubleRange OffsetBounds => _offsetBounds;

		public double Rtt => _rttAverage.Value;

		public event Action Sent;
		public event OnPongReceived Received;

		public ServerTime(ManualLogSource log, Peer server, double interval, Timestamped<PingMessage> seed)
		{
			_log = log;
			_server = server;

			_pingTimer = new LoopTimer(interval);

			var offset = ProcessPong(seed, out var rtt, out _offsetBounds);

			_offsetAverage = new ExponentialMovingAverage(offset, EMA_ALPHA);
			_rttAverage = new ExponentialMovingAverage(rtt, EMA_ALPHA);
		}

		/// <summary>
		///		Start an offset update if enough time has passed.
		/// </summary>
		/// <param name="server">The server peer.</param>
		public void Update()
		{
			if (!_pingTimer.TryCycle())
			{
				return;
			}

			_server.Send(PingMessage.Now);

			Sent?.Invoke();
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

			Received?.Invoke(offset, rtt);
		}
	}
}
