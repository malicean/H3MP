using H3MP.Client.Utils;
using H3MP.Common.Utils;

using BepInEx;

using LiteNetLib;
using LiteNetLib.Utils;
using BepInEx.Configuration;
using H3MP.Common.Messages;
using System.Collections;
using UnityEngine;

namespace H3MP.Client
{
	[BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly ConfigEntry<string> _configAddress;
		private readonly ConfigEntry<ushort> _configPort;
		private readonly ConfigEntry<string> _configPassword;

		public Pool<NetDataWriter> Writers { get; }

		public NetworkTime Time { get; }

		public NetManager Client { get; }

		public NetPeer Server { get; private set; }

		public Plugin()
		{
			_configAddress = Config.Bind("h3mp", "address", "localhost", "Address to connect to");
			_configPort = Config.Bind("h3mp", "port", (ushort) 7777, "Port to connect to");
			_configPassword = Config.Bind("h3mp", "passphrase", (string) null, "Passphrase (if any) to connect with");

			Writers = new Pool<NetDataWriter>(new NetDataWriterPoolSource());
			Time = new NetworkTime(Logger, Writers);

			var listener = new NetEventListener(Logger, Time);
			Client = new NetManager(listener);
		}

		private void Start()
		{
			Client.Start();

			Writers.Borrow(out var writer);
			writer.Put(new ConnectionData(_configPassword.Value));

			Server = Client.Connect(_configAddress.Value, _configPort.Value, writer);

			StartCoroutine(nameof(_TestPings));
		}

		private IEnumerator _TestPings()
		{
			for (var i = 0; i < 10; ++i)
			{
				Time.StartUpdate(Server);

				yield return new WaitForSeconds(5f);
			}
		}

		private void Update()
		{
			Client.PollEvents();
		}

		private void OnDestroy()
		{
			Client.Stop();
		}
	}
}
