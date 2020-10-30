using System;
using System.Text;
using BepInEx.Logging;
using H3MP.Time;
using LiteNetLib;

namespace H3MP.Peers
{
	public abstract class Peer : IFixedUpdatable, IDisposable
    {
		private readonly LoopTimer _tickTimer;
#if DEBUG
		private readonly LoopTimer _statsTimer;
#endif

		protected readonly ManualLogSource Log;
		protected readonly EventBasedNetListener Listener;
		protected readonly NetManager Net;

		public Peer(ManualLogSource log, double tickStep)
		{
			Log = log;
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
		}

		protected virtual bool NetUpdate()
		{
			if (!_tickTimer.TryCycle(LocalTime.Now))
			{
				return false;
			}

			Net.PollEvents();

			return true;
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
