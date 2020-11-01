using System;
using System.Collections;
using System.Net;
using H3MP.Configs;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Peers;
using H3MP.Timing;
using H3MP.Utils;
using UnityEngine;

namespace H3MP
{
	public class PeerManager : IStartable, IFixedUpdatable, IDisposable
	{
		private readonly PeerLogs _logs;
		private readonly PeerConfig _config;
		private readonly Version _version;
		private readonly Func<IEnumerator, Coroutine> _startCoroutine;

		public Option<Server> Server { get; private set; }

		public Option<Client> Client { get; private set; }

		public event Action<Server> ServerCreated;
		public event Action<Client> ClientCreated;

		public event Action<Server> ServerKilled;
		public event Action<Client> ClientKilled;

		public PeerManager(PeerLogs logs, PeerConfig config, Version version, Func<IEnumerator, Coroutine> startCoroutine)
		{
			_logs = logs;
			_config = config;
			_version = version;
			_startCoroutine = startCoroutine;

			Server = Option.None<Server>();
			Client = Option.None<Client>();
		}

		private void KillServer()
		{
			if (!Server.MatchSome(out var server))
			{
				return;
			}

			_logs.Manager.Common.LogDebug("Killing server...");

			ServerKilled?.Invoke(server);
			server.Dispose();
			Server = Option.None<Server>();
		}

		private void KillClient()
		{
			if (!Client.MatchSome(out var client))
			{
				return;
			}

			_logs.Manager.Common.LogDebug("Killing client...");

			ClientKilled?.Invoke(client);
			client.Dispose();
			Client = Option.None<Client>();
		}

		private void KillAllPeers()
		{
			_logs.Manager.Common.LogDebug("Killing peers...");

			KillServer();
			KillClient();
		}

		private IEnumerator _HostUnsafe()
		{
			var log = _logs.Server;
			log.Common.LogDebug("Starting server...");

			var config = _config.Host;

			var binding = config.Binding;
			var ipv4 = binding.IPv4.Value;
			var ipv6 = binding.IPv6.Value;
			var port = binding.Port.Value;
			var localhost = new IPEndPoint(ipv4 == IPAddress.Any ? IPAddress.Loopback : ipv4, port);

			IPEndPoint publicEndPoint;
			{
				log.Common.LogDebug("Awaiting public address...");
				IPAddress publicAddress;
				{
					var getter = config.PublicBinding.GetAddress();
					foreach (object o in getter._Run()) yield return o;

					var result = getter.Result;

					if (!result.Key)
					{
						log.Common.LogFatal($"Failed to get public IP address to host server with: {result.Value}");
						yield break;
					}

					// Safe to parse, already checked by AddressGetter
					publicAddress = IPAddress.Parse(result.Value);
				}

				ushort publicPort = config.PublicBinding.Port.Value;
				if (publicPort == 0)
				{
					publicPort = port;
				}

				publicEndPoint = new IPEndPoint(publicAddress, publicPort);
			}


			float ups = 1 / Time.fixedDeltaTime; // 90
			double tps = config.TickRate.Value;
			if (tps <= 0)
			{
				log.Common.LogFatal("The configurable tick rate must be a positive value.");
				yield break;
			}

			if (tps > ups)
			{
				tps = ups;
				log.Common.LogWarning($"The configurable tick rate ({tps:.00}) is greater than the local fixed update rate ({ups:.00}). The config will be ignored and the fixed update rate will be used instead; running a tick rate higher than your own fixed update rate has no benefits.");
			}

			double tickDeltaTime = 1 / tps;

			var server = new Server(_logs.Server, _config.Host, tickDeltaTime, _version, publicEndPoint);
			Server = Option.Some(server);
			ServerCreated?.Invoke(server);

			log.Common.LogDebug("Binding server...");
			server.Start(_config.Host.Binding.Structured);

			if (log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogInfo($"Now hosting on {publicEndPoint}!");
			}
			else
			{
				log.Common.LogInfo("Now hosting!");
			}

			ConnectLocal(localhost, server.LocalSnapshot.Secret, server.AdminKey);
		}

		private IEnumerator _Host()
		{
			KillAllPeers();

			return _HostUnsafe();
		}

		private Client Connect(IPEndPoint endPoint, JoinSecret secret, ConnectionRequestMessage request)
		{
			var log = _logs.Client;
			if (log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogInfo($"Connecting to {endPoint}...");
			}
			else
			{
				log.Common.LogInfo("Connecting...");
			}

			float ups = 1 / Time.fixedDeltaTime;
			double tps = 1 / secret.TickStep;

			log.Common.LogDebug($"Fixed update rate: {ups:.00} u/s");
			log.Common.LogDebug($"Tick rate: {tps:.00} t/s");

			var client = new Client(_logs.Client, _config.Client, secret.TickStep, secret.MaxPlayers);
			Client = Option.Some(client);
			ClientCreated?.Invoke(client);

			client.Connect(endPoint, request);
			return client;
		}

		private void ConnectLocal(IPEndPoint endPoint, JoinSecret secret, Key32 adminKey)
		{
			var client = Connect(endPoint, secret, new ConnectionRequestMessage(true, adminKey, _version));

			client.Disconnected += info =>
			{
				_logs.Client.Common.LogError("Disconnected from local server. Something probably caused the frame to hang for more than 5s (debugging breakpoint?). Restarting host...");

				_startCoroutine(_Host());
			};
		}

		public void Start()
		{
			if (_config.AutoHost.Value)
			{
				_logs.Manager.Common.LogDebug("Autostarting host from game launch...");

				_startCoroutine(_Host());
			}
		}

		public void FixedUpdate()
		{
			if (Client.MatchSome(out var client))
			{
				client.FixedUpdate();
			}

			if (Server.MatchSome(out var server))
			{
				server.FixedUpdate();
			}
		}

		public void Connect(JoinSecret secret)
		{
			KillAllPeers();

			var client = Connect(secret.EndPoint, secret, new ConnectionRequestMessage(false, secret.Key, _version));

			client.Disconnected += info =>
			{
				_logs.Client.Common.LogError("Disconnected from remote server: " + info.Reason);

				if (_config.AutoHost.Value)
				{
					_logs.Manager.Common.LogDebug("Autostarting host from client disconnection...");

					_startCoroutine(_Host());
				}
			};
		}

		public void Dispose()
		{
			KillAllPeers();
		}
	}
}
