using BepInEx;
using DiscordRPC;
using H3MP.Utils;
using System.Security.Cryptography;
using H3MP.Networking;
using BepInEx.Configuration;
using System.Collections;
using UnityEngine.Networking;
using System.Net;
using H3MP.Messages;
using LiteNetLib;
using H3MP.HarmonyPatches;
using UnityEngine;
using DiscordRPC.Message;
using System;
using HarmonyLib;

namespace H3MP
{
    [BepInPlugin("ash.h3mp", "H3VR Multiplayer", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private const string DISCORD_APP_ID = "762557783768956929"; // 3rd party RPC application
		private const string STEAM_APP_ID = "450540"; // H3VR

		// Unity moment
		public static Plugin Instance { get; private set; }

		private static readonly UniversalMessageList<H3Client, H3Server> _messages = new UniversalMessageList<H3Client, H3Server>()
			// =======
			// Client
			// =======
			// Dedicated time synchronization
			.AddClient<PingMessage>(0, DeliveryMethod.Sequenced, H3Server.OnClientPing)
			// =======
			// Server
			// =======
			// Dedicated time synchronization
			.AddServer<PongMessage>(0, DeliveryMethod.Sequenced, H3Client.OnServerPong)
			// Party management
			.AddServer<PartyInitMessage>(1, DeliveryMethod.ReliableOrdered, H3Client.OnServerPartyInit)
			.AddServer<PartyChangeMessage>(1, DeliveryMethod.ReliableOrdered, H3Client.OnServerPartyChange)
			// Asset management
			.AddServer<LevelChangeMessage>(2, DeliveryMethod.ReliableOrdered, H3Client.OnLevelChange);

		private static readonly MessageDefinition _pongDefinition = _messages.Server.Definitions[typeof(PongMessage)];

		private readonly RootConfig _config;

		private readonly System.Version _version;
		private readonly RichPresence _presence;
		private readonly RandomNumberGenerator _rng;

		public DiscordRpcClient Discord { get; }

		public H3Server Server { get; private set; }

		public H3Client Client { get; private set; }

		public Plugin()
		{
			_version = Info.Metadata.Version;

			Logger.LogDebug("Binding configs...");
			{
				TomlTypeConverter.AddConverter(typeof(IPAddress), new TypeConverter
				{
					ConvertToObject = (raw, type) => IPAddress.Parse(raw),
					ConvertToString = (value, type) => ((IPAddress) value).ToString()
				});

				_config = new RootConfig(Config, "H3MP");
			}

			Logger.LogDebug("Initializing utilities...");
			{
				_rng = RandomNumberGenerator.Create();
			}

			Logger.LogDebug("Initializing Discord rich presence...");
			{
				_presence = new RichPresence();
				Discord = new DiscordRpcClient(DISCORD_APP_ID, autoEvents: false, logger: new DiscordLog(Logger));

				Discord.OnJoinRequested += OnJoinRequested;
				Discord.OnJoin += OnJoin;

				Discord.RegisterUriScheme(STEAM_APP_ID);
				Discord.SetSubscription(DiscordRPC.EventType.Join);
				Discord.SetPresence(_presence);
				Discord.Initialize();
			}
		}

        private void OnJoin(object sender, JoinMessage args)
        {
            const string errorPrefix = "Failed to handle Discord join event: ";

            var raw = args.Secret;
            Logger.LogDebug($"Received Discord join secret \"{raw}\"");

            bool success = JoinSecret.TryParse(raw, out var secret, out var version);
            if (!_version.CompatibleWith(version))
            {
                Logger.LogError(errorPrefix + $"version incompatibility detected (you: {_version}; host: {version})");
                return;
            }

            if (!success)
            {
                Logger.LogError(errorPrefix + $"failed to parse join secret \"{raw}\"");
                return;
            }

			ConnectRemote(secret);
        }

        private void OnJoinRequested(object sender, JoinRequestMessage args)
        {
            // All friends can join
            // TODO: Change this.
            Discord.Respond(args, true);
        }

        private IEnumerator _HostUnsafe()
		{
			var config = _config.Host;

			var binding = config.Binding;
			var ipv4 = binding.IPv4.Value;
			var ipv6 = binding.IPv6.Value;
			var port = binding.Port.Value;
			var localhost = new IPEndPoint(ipv4 == IPAddress.IPv6Any ? IPAddress.IPv6Loopback : ipv4, port);

			IPEndPoint publicEndPoint;
			{
				IPAddress publicAddress;
				{
					var getter = config.Public.GetAddress();
					foreach (object o in getter._Run()) yield return o;

					var result = getter.Result;
					
					if (!result.Key) 
					{
						Logger.LogFatal($"Failed to get public IP address to host server with: {result.Value}");
						yield break;
					}

					// Safe to parse, already checked by AddressGetter
					publicAddress = IPAddress.Parse(result.Value);
				}

				ushort publicPort = config.Public.Port.Value;
				if (publicPort == 0)
				{
					publicPort = port;
				}

				publicEndPoint = new IPEndPoint(publicAddress, publicPort);
			}

			Server = new H3Server(Logger, _rng, _messages.Server, _version, _config.Host, publicEndPoint);
			Logger.LogInfo($"Now hosting on {publicEndPoint}!");

			ConnectLocal(localhost, Server.Secret.Key);
		}

		private IEnumerator _Host()
		{
			Logger.LogDebug("Killing peers...");

			Client?.Dispose();
			Client = null;

			Server?.Dispose();
			Server = null;

			return _HostUnsafe();
		}

		private void Connect(IPEndPoint endPoint, Key32 key, OnH3ClientDisconnect onDisconnect)
        {
            Client = new H3Client(Logger, Discord, _messages.Client, _version, endPoint, new ConnectionRequestMessage(key), onDisconnect);
        }

		private void ConnectLocal(IPEndPoint endPoint, Key32 key)
		{
			Connect(endPoint, key, info => 
			{
				Logger.LogError("Disconnected from local server. This should never happen. Restarting host...");
				
				StartCoroutine(_Host());
			});
		}

		private void ConnectRemote(JoinSecret secret)
		{
			Client?.Dispose();
			Connect(secret.EndPoint, secret.Key, info => 
			{
				Logger.LogError("Disconnected from remote server.");
				Logger.LogDebug("Killing client...");

				Client?.Dispose();
				Client = null;

				if (_config.AutoHost.Value) 
				{
					Logger.LogDebug("Autostarting host from client disconnection...");

					StartCoroutine(_Host());
				}
			});
		}

		private void Awake()
		{
			Instance = this;

			new Harmony(Info.Metadata.GUID).PatchAll();
		}

		private void Start()
		{
			if (_config.AutoHost.Value) 
			{
				Logger.LogDebug("Autostarting host from game launch...");

				StartCoroutine(_Host());
			}
		}

		private void Update()
		{
			Discord.Invoke();

			Client?.Update();
			Server?.Update();
		}

		private void OnDestroy()
		{
			Discord.Dispose();

			Server?.Dispose();
			Client?.Dispose();
		}
    }
}
