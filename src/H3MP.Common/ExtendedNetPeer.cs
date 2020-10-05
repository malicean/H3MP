using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Common
{
	public class ExtendedNetPeer
	{
		private

		public NetPeer Peer { get; }

		private class DisposableNetDataWriter : Netdatawr, IDisposable
		{
			public void Dispose()
			{
				throw new NotImplementedException();
			}
		}
	}
}
