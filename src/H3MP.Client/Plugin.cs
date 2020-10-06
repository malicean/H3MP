using H3MP.Client.Utils;

using H3MP.Common.Utils;
using H3MP.Common.Messages;

using System.Collections;

using BepInEx;
using BepInEx.Configuration;

using LiteNetLib;
using LiteNetLib.Utils;

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
			Logger.LogDebug("Binding configs...");
			_configAddress = Config.Bind("h3mp", "address", "localhost", "Address to connect to");
			_configPort = Config.Bind("h3mp", "port", (ushort) 7777, "Port to connect to");
			_configPassword = Config.Bind("h3mp", "passphrase", string.Empty, "Passphrase (if any) to connect with");

			Logger.LogDebug("Initializing utilities...");
			Writers = new Pool<NetDataWriter>(new NetDataWriterPoolSource());
			Time = new NetworkTime(Logger, Writers);

			Logger.LogDebug("Initializing network...");
			var listener = new NetEventListener(Logger, Time);
			Client = new NetManager(listener);
		}

		private void Start()
		{
			Logger.LogDebug("Starting network...");
			Client.Start();

			Writers.Borrow(out var writer);
			writer.Put(new ConnectionData(_configPassword.Value));

			var address = _configAddress.Value;
			var port = _configPort.Value;
			Logger.LogDebug($"Connecting to {address}:{port}...");
			Server = Client.Connect(address, port, writer);

			Logger.LogDebug("Running tests...");
			StartCoroutine(nameof(_TestPings));
		}

		private IEnumerator _TestPings()
		{
			for (var i = 0; i < 10; ++i)
			{
				Logger.LogDebug($"Test iteration {i}...");
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
