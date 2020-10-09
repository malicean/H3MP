using BepInEx.Logging;
using H3MP.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP
{
	public class NetManager : IDisposable
	{
		private static Dictionary<Type, MessageDefinition> _messageDefinitions = new Dictionary<Type, MessageDefinition>
		{
			// client
			[typeof(PongMessage)] = MessageDefinition.New<PongMessage>(0, DeliveryMethod.Sequenced, 0, ClientPong),
			[typeof(LevelChangeMessage)] = MessageDefinition.New<LevelChangeMessage>(1, DeliveryMethod.ReliableSequenced, 0, ClientLevelChange),

			// server
			[typeof(PingMessage)] = MessageDefinition.New<PingMessage>(128, DeliveryMethod.Sequenced, 0, ServerPing),
		};

		private ManualLogSource _log;
		private ServerTime _time;
		private Client _client;

		private Server _server;

		public void Host(ushort port)
		{
			_log.LogDebug("Constructing server dependencies...");
			var key = ConnectionKey.FromRandom(_rng);
			var pong = _messageDefinitions[typeof(PongMessage)];
			var events = new MessageServerEvents(_log, pong, key);

			_log.LogDebug("Constructing server...");
			_client = null;
			_server = new Server(_log, _messageDefinitions, events, _version, port);

			_log.LogInfo($"Now hosting on port {port}!");
		}

		public void Connect(string address, ushort port, ConnectionKey key)
		{
			_log.LogDebug("Constructing client dependencies...");
			var events = new MessageClientEvents();

			_log.LogDebug("Constructing client...");
			_client = new Client(_log, _messageDefinitions, events, _version, address, port, x => x.Put(new ConnectionRequestMessage(key)));
			_server = null;
		}
	}
}
