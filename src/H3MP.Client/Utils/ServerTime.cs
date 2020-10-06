using H3MP.Client.Extensions;
using H3MP.Common.Messages;
using H3MP.Common.Utils;

using BepInEx.Logging;

using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace H3MP.Client.Utils
{
	// Heavily inspired by Mirror Networking for Unity (I loved using this feature).
	// Sources:
	//	- Mirror (abstract): https://mirror-networking.com/docs/Articles/Guides/ClockSync.html
	//	- Mirror (source): https://github.com/vis2k/Mirror/blob/master/Assets/Mirror/Runtime/NetworkTime.cs
	/// <summary>
	///		Synchronizes the server-localized time in real time on the client.
	/// </summary>
	public class ServerTime
	{
		/// <summary>
		///		Determines the stability of the offset before it is "squeezed" (technically never, but practically when the range is 2ms).
		///		Range is (0, 1).
		///		When it is >0.5, latest offset matters more.
		///		When it is <0.5, history offset matters more.
		/// </summary>
		public const double OFFSET_EMA_ALPHA = 2d / 10;

		private readonly ManualLogSource _logger;
		private readonly Pool<NetDataWriter> _writers;
		private readonly ExponentialMovingAverage _offsetAverage;

		private double _offset;
		private double _offsetMin;
		private double _offsetMax;

		/// <summary>
		///		The <see cref="LocalTime.Now" /> value now, in real time, on the server.
		/// </summary>
		public double Now => LocalTime.Now - _offset;

		public ServerTime(ManualLogSource logger, Pool<NetDataWriter> writers) 
		{
			_logger = logger;
			_writers = writers;
			_offsetAverage = new ExponentialMovingAverage(OFFSET_EMA_ALPHA);

			ResetBounds();
		}

		private void UpdateOffset(double value)
		{
			_offset = _offsetAverage.Push(value);
		}

		/// <summary>
		///		Resets the range the offset is permitted to be.
		///		This should only be used if the remote time is changed at a non-standard (doesn't match with time) rate.
		///		The primary use case is a synchronizing with a new server.
		/// </summary>
		public void ResetBounds()
		{
			_offsetMin = double.NegativeInfinity;
			_offsetMax = double.PositiveInfinity;
		}

		/// <summary>
		///		Start an offset update.
		/// </summary>
		/// <param name="server">The server peer.</param>
		public void StartUpdate(NetPeer server)
		{
			_writers.Borrow(out var writer);
			writer.PutTyped(new PingMessage(LocalTime.Now));

			server.Send(writer, DeliveryMethod.ReliableSequenced);
		}

		// This is almost a direct rip from Mirror's NetworkTime.OnClientPong(...), but at least I understand how it works.
		/// <summary>
		///		Finishes an offset update.
		/// </summary>
		/// <param name="pong">The times sent by the server, including the original send time.</param>
		public void FinishUpdate(PongMessage pong)
		{
			var now = LocalTime.Now;

			// The difference between the reply time locally (estimated) and remotely (known).
			// Calculated by getting the instant equally between when the message was sent and when it was received.
			var offset = pong.ClientTime +
				(now - pong.ClientTime) * 0.5 // estimated time until server response
				- pong.ServerTime;

			// Basically squeeze theorem: if we absolutely know the range of the real offset, keep shrinking it until the difference is infinitely small (epsilon).
			// Very important to reliability of the offset. Without this, I (Ash) was getting +/- 15ms offsets every few samples. With it, +/- <1ms after ~30 samples.
			_offsetMin = Math.Max(_offsetMin, pong.ClientTime - pong.ServerTime);   // The known minimum offset. It is impossible for the estimated offset to be lower.
			_offsetMax = Math.Min(_offsetMax, now - pong.ServerTime);				// The known maximum offset. It is impossible for the estimated offset to be higher.

			if (_offset < _offsetMin || _offsetMax < _offset) // Old offset is out of bounds.
			{
				// Moving average is out of known bounds. Reset it.
				_offsetAverage.Reset();

				// The new value is guaranteed to be more accurate on one side, so seed the new average using it.
				UpdateOffset(offset);
			}
			else if (_offsetMin <= offset && offset <= _offsetMax) // New offset is within bounds.
			{
				UpdateOffset(offset);
			}

			_logger.LogDebug($"RTT: {(now - pong.ClientTime) * 0.5:.000}s; Offset info: {_offsetMin:.000}s <= est. {_offset:.000}s <= {_offsetMax:.000}s");
		}
	}
}
