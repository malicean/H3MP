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
		private static Stopwatch _watch = Stopwatch.StartNew();

		public static double Value { get; private set; }

		public void StartUpdate(NetPeer peer)
		{
			var writer = new NetDataWriter();

			peer.Send(, DeliveryMethod.ReliableSequenced);
		}
	}
}
