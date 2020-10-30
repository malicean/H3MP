using System;
using BepInEx.Logging;
using H3MP.Differentiation;
using H3MP.IO;
using H3MP.Time;
using H3MP.Utils;
using LiteNetLib;

namespace H3MP.Peers
{
	public abstract class Peer<TLocalSnapshot, TDeltaLocalSnapshot, TConfig> : IFixedUpdatable, IDisposable where TLocalSnapshot : ICopyable<TLocalSnapshot>, new()
    {
		private readonly LoopTimer _tickTimer;
#if DEBUG
		private readonly LoopTimer _statsTimer;
#endif

		private readonly IDifferentiator<TLocalSnapshot, TDeltaLocalSnapshot> _localSnapshotDifferentiator;

		protected readonly ManualLogSource Log;
		protected readonly TConfig Config;

		protected readonly EventBasedNetListener Listener;
		protected readonly NetManager Net;

		protected readonly double TickStep;
		protected uint Tick { get; private set; }

		private Option<TLocalSnapshot> _localSnapshotOld;
		public TLocalSnapshot LocalSnapshot;

		public event Action<Exception> UnhandledReadException;
		public event Action UnreadData;

		public Peer(ManualLogSource log, TConfig config, double tickStep, IDifferentiator<TLocalSnapshot, TDeltaLocalSnapshot> localSnapshotDifferentiator)
		{
			_localSnapshotDifferentiator = localSnapshotDifferentiator;

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

			LocalSnapshot = new TLocalSnapshot();

			Listener.NetworkReceiveEvent += NetworkReceived;
		}

		private Option<TDeltaLocalSnapshot> PopDelta()
		{
			var delta = _localSnapshotDifferentiator.CreateDelta(LocalSnapshot, _localSnapshotOld);

			_localSnapshotOld = Option.Some(LocalSnapshot.Copy());

			return delta;
		}

		private void NetUpdate()
		{
			if (!_tickTimer.TryCycle(LocalTime.Now))
			{
				return;
			}

			Net.PollEvents();
			OnNetUpdate();

			if (PopDelta().MatchSome(out var delta))
			{
				SendDelta(delta);
			}

			++Tick;
		}

#if DEBUG
		private void StatsUpdate()
		{
			if (_statsTimer.TryCycle(LocalTime.Now))
			{
				return;
			}

			Log.LogDebug("\n" + Net.Statistics.ToString());
		}
#endif

		private void NetworkReceived(NetPeer peer, NetPacketReader data, DeliveryMethod deliveryMethod)
		{
			var reader = new BitPackReader(data);

			try
			{
				ReceiveDelta(peer, ref reader);
			}
			catch (Exception e)
			{
				UnhandledReadException?.Invoke(e);
				return;
			}

			if (!reader.Bits.Done || !reader.Bytes.Done)
			{
				UnreadData?.Invoke();
			}
		}

		protected abstract void ReceiveDelta(NetPeer peer, ref BitPackReader reader);

		protected abstract void SendDelta(TDeltaLocalSnapshot delta);

		protected virtual void OnNetUpdate() { }

        public virtual void FixedUpdate()
        {
			NetUpdate();
#if DEBUG
			StatsUpdate();
#endif
        }

        public void Dispose()
        {
			Net.DisconnectAll();
            Net.Stop();
        }
    }
}
