using H3MP.Client.Utils;
using H3MP.Common.Utils;

using BepInEx;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Client
{
	[BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public Pool<NetDataWriter> Writers { get; private set; }

		public NetworkTime Time { get; private set; }

		public NetManager Network { get; private set; }

		private void Awake()
		{
			Writers = new Pool<NetDataWriter>(new NetDataWriterPoolSource());
			Time = new NetworkTime(Logger, Writers);

			var listener = new NetEventListener(Logger, Time);
			Network = new NetManager(listener);
		}

		private void Update()
		{
			Network.PollEvents();
		}
	}
}
