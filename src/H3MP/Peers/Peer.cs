using System;
using H3MP.IO;
using H3MP.Timing;
using H3MP.Utils;
using LiteNetLib;

namespace H3MP.Peers
{
	public abstract class Peer<TSelf, TSnapshot, TConfig> : IDisposable where TSnapshot : ICopyable<TSnapshot>, new()
    {
		public delegate void SimulateHandler(TSelf self);
		public delegate void CleanupHandler(TSelf self);
		public delegate void DataUnreadHandler(NetPeer peer);

		private bool _disposed;

		protected readonly TickFrameClock Clock;
		protected readonly Log Log;
		protected readonly TConfig Config;

		protected readonly EventBasedNetListener Listener;
		protected readonly NetManager Net;

		protected abstract TSelf Self { get; }

		public TSnapshot LocalSnapshot;

		public double TickStep => Clock.DeltaTime;

		public event SimulateHandler Simulate;
		public event CleanupHandler Cleanup;
		public event DataUnreadHandler DataUnread;

		public Peer(Log log, TConfig config, TickFrameClock clock)
		{
			Log = log;
			Config = config;
			Clock = clock;

			Listener = new EventBasedNetListener();
			Net = new NetManager(Listener)
			{
				AutoRecycle = true
			};

			LocalSnapshot = new TSnapshot();

			Listener.PeerConnectedEvent += Connected;
			Listener.PeerDisconnectedEvent += Disconnected;
			Listener.NetworkReceiveEvent += NetworkReceived;

			Clock.Elapsed += RunTick;
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

		private void NetworkReceived(NetPeer peer, NetPacketReader data, DeliveryMethod deliveryMethod)
		{
			var reader = new BitPackReader(data);

			ReceiveDelta(peer, ref reader);

			if (!reader.Bits.Done || !reader.Bytes.Done)
			{
				DataUnread?.Invoke(peer);
			}
		}

		protected abstract void ReceiveDelta(NetPeer peer, ref BitPackReader reader);

		protected abstract void SendSnapshot(TSnapshot snapshot);

		public void RunTick()
		{
			Net.PollEvents();
			Simulate?.Invoke(Self);
			SendSnapshot(LocalSnapshot);
			Cleanup?.Invoke(Self);
		}

		protected virtual void DisposeSafe()
		{
			Clock.Elapsed -= RunTick;

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
