using H3MP.Client.Utils;

using H3MP.Common.Utils;
using H3MP.Common.Messages;

using BepInEx;
using BepInEx.Configuration;

using LiteNetLib;
using LiteNetLib.Utils;

namespace H3MP.Client
{
	[BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly ConfigEntry<string> _configAddress;
		private readonly ConfigEntry<ushort> _configPort;
		private readonly ConfigEntry<string> _configPassword;

		private NetEventListener _listener;
		private NetManager _client;

		public Pool<NetDataWriter> Writers { get; }

		public NetPeer Server => _listener.Server;

		public double Time => _listener.Time;

		public Plugin()
		{
			Logger.LogDebug("Binding configs...");
			_configAddress = Config.Bind("H3MP", "address", "localhost", "Address to connect to");
			_configPort = Config.Bind("H3MP", "port", (ushort) 7777, "Port to connect to");
			_configPassword = Config.Bind("H3MP", "passphrase", string.Empty, "Passphrase (if any) to connect with");

			Logger.LogDebug("Initializing utilities...");
			Writers = new Pool<NetDataWriter>(new NetDataWriterPoolSource());

			Logger.LogDebug("Initializing network...");
			_listener = new NetEventListener(Logger, Writers);
			_client = new NetManager(_listener);
		}

		public void Connect(string address, int port, string password)
		{
			Writers.Borrow(out var writer);
			writer.Put(new ConnectionRequestMessage(_configPassword.Value));

			Logger.LogDebug($"Connecting to {address}:{port}...");
			_client.Connect(address, port, writer);
		}

		private void Start()
		{
			Logger.LogDebug("Starting network...");
			_client.Start();

			Connect(_configAddress.Value, _configPort.Value, _configPassword.Value);
		}

		private void Update()
		{
			_client.PollEvents();

			_listener.Update();
		}

		private void OnDestroy()
		{
			_client.Stop();
		}
	}
}
