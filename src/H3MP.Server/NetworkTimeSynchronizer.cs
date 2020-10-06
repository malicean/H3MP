using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace H3MP.Common
{
	public class NetworkTimeSynchronizer
	{
		private readonly static Stopwatch _watch = Stopwatch.StartNew();

		private readonly Pool<NetDataWriter> _pool;

		public double Value { get; private set; }

		public NetworkTimeSynchronizer(Pool<NetDataWriter> pool) 
		{
			_pool = pool;
		}

		public void StartUpdate(NetPeer peer)
		{

		}
	}
}
