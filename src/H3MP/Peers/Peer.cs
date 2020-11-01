using System;
using BepInEx.Logging;
using H3MP.Differentiation;
using H3MP.IO;
using H3MP.Timing;
using H3MP.Utils;
using LiteNetLib;

namespace H3MP.Peers
{
	public abstract class Peer<TSnapshot, TConfig> : IFixedUpdatable, IDisposable where TSnapshot : ICopyable<TSnapshot>, new()
    {
		private readonly LoopTimer _tickTimer;
#if DEBUG
		private readonly LoopTimer _statsTimer;
#endif

		private bool _disposed;

		protected readonly Log Log;
		protected readonly TConfig Config;

		protected readonly EventBasedNetListener Listener;
		protected readonly NetManager Net;

		public readonly double TickStep;
		public uint Tick { get; private set; }

		public TSnapshot LocalSnapshot;

		public event Action Ticked;
		public event Action DataUnread;

		public Peer(Log log, TConfig config, double tickStep)
		{
			Log = log;
			Config = config;

			Listener = new EventBasedNetListener();
			Net = new NetManager(Listener)
			{
				AutoRecycle = true,
#if DEBUG
				EnableStatistics = true
#endif
			};

			_tickTimer = new LoopTimer(tickStep, LocalTime.Now);
#if DEBUG
			_statsTimer = new LoopTimer(60, LocalTime.Now);
#endif

			TickStep = tickStep;
			Tick = 0;

			LocalSnapshot = new TSnapshot();

			Listener.NetworkReceiveEvent += NetworkReceived;
			Listener.PeerConnectedEvent += Connected;
			Listener.PeerDisconnectedEvent += Disconnected;
		}

		private void Connected(NetPeer peer)
		{
			if (Log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogInfo($"Peer ({peer.Id}; {peer.EndPoint}) connected!");
			}
			else
			{
				Log.Common.LogInfo($"Peer ({peer.Id}) connected!");
			}
		}

		private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			if (Log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogInfo($"Peer ({peer.Id}; {peer.EndPoint}) disconnected!");
			}
			else
			{
				Log.Common.LogInfo($"Peer ({peer.Id}) disconnected!");
			}
		}

		private void NetUpdate()
		{
			if (!_tickTimer.TryCycle(LocalTime.Now))
			{
				return;
			}

			Net.PollEvents();
			Ticked?.Invoke();
			SendSnapshot(LocalSnapshot);

			++Tick;
		}

#if DEBUG
		private void StatsUpdate()
		{
			if (!_statsTimer.TryCycle(LocalTime.Now))
			{
				return;
			}

			Log.Common.LogDebug("\n" + Net.Statistics.ToString());
			Net.Statistics.Reset();
		}
#endif

		private void NetworkReceived(NetPeer peer, NetPacketReader data, DeliveryMethod deliveryMethod)
		{
			var reader = new BitPackReader(data);

			ReceiveDelta(peer, ref reader);

			if (!reader.Bits.Done || !reader.Bytes.Done)
			{
				DataUnread?.Invoke();
			}
		}

		protected abstract void ReceiveDelta(NetPeer peer, ref BitPackReader reader);

		protected abstract void SendSnapshot(TSnapshot snapshot);

        public virtual void FixedUpdate()
        {
			NetUpdate();
#if DEBUG
			StatsUpdate();
#endif
        }

		protected virtual void DisposeSafe()
		{
			Net.DisconnectAll();
			Net.Stop();
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			DisposeSafe();

			_disposed = true;
		}
	}
}
