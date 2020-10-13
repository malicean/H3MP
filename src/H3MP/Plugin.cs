using BepInEx;
using Discord;
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
using System;
using HarmonyLib;
using BepInEx.Logging;
using System.Linq;
using System.Runtime.InteropServices;
using H3MP.Peers;
using H3MP.Models;

namespace H3MP
{
    [BepInPlugin("ash.h3mp", "H3MP", "0.0.0")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		[DllImport("kernel32.dll")]
    	private static extern IntPtr LoadLibrary(string path);

		private const long DISCORD_APP_ID = 762557783768956929; // 3rd party RPC application
		private const uint STEAM_APP_ID = 450540; // H3VR

		// Unity moment
		public static Plugin Instance { get; private set; }

		private readonly System.Version _version;
		private readonly string _name;

		private readonly RootConfig _config;

		private readonly RandomNumberGenerator _rng;

		private readonly ManualLogSource _clientLog;
		private readonly ManualLogSource _serverLog;
		private readonly ManualLogSource _discordLog;
		private readonly ManualLogSource _harmonyLog;

		private readonly UniversalMessageList<H3Client, H3Server> _messages;

		internal ManualLogSource HarmonyLogger => _harmonyLog;

        public Discord.Discord DiscordClient { get; }

		public ActivityManager ActivityManager { get; }

		public StatefulActivity Activity { get; }

		public H3Server Server { get; private set; }

		public H3Client Client { get; private set; }

        public Plugin()
		{
			_version = Info.Metadata.Version;
			_name = Info.Metadata.Name;

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

				_clientLog = BepInEx.Logging.Logger.CreateLogSource(_name + "-CL");
				_serverLog = BepInEx.Logging.Logger.CreateLogSource(_name + "-SV");
				_discordLog = BepInEx.Logging.Logger.CreateLogSource(_name + "-DC");
				_harmonyLog = BepInEx.Logging.Logger.CreateLogSource(_name + "-HM");
			}

			Logger.LogDebug("Initializing Discord game SDK...");
			{
				// TODO: when BepInEx next releases (>5.3), uncomment this line and move discord_game_sdk.dll to the plugin folder
				// LoadLibrary("BepInEx\\plugins\\H3MP\\" + Discord.Constants.DllName + ".dll");

				DiscordClient = new Discord.Discord(DISCORD_APP_ID, (ulong) CreateFlags.Default);
				DiscordClient.SetLogHook(Discord.LogLevel.Debug, (level, message) => 
				{
					switch (level)
					{
						case Discord.LogLevel.Error:
							_discordLog.LogError(message);
							break;
						case Discord.LogLevel.Warn:
							_discordLog.LogWarning(message);
							break;
						case Discord.LogLevel.Info:
							_discordLog.LogInfo(message);
							break;
						case Discord.LogLevel.Debug:
							_discordLog.LogDebug(message);
							break;

						default:
							throw new NotImplementedException(level.ToString());
					}
				});

				ActivityManager = DiscordClient.GetActivityManager();
				Activity = new StatefulActivity(ActivityManager, DiscordCallbackHandler);

				ActivityManager.RegisterSteam(STEAM_APP_ID);

				ActivityManager.OnActivityJoinRequest += OnJoinRequested;
				ActivityManager.OnActivityJoin += OnJoin;
			}

			Logger.LogDebug("Creating message table...");
			{
				_messages = new UniversalMessageList<H3Client, H3Server>(_clientLog, _serverLog)
					// =======
					// Client
					// =======
					// Time synchronization
					.AddClient<PingMessage>(0, DeliveryMethod.Sequenced, H3Server.OnClientPing)
					// Player movement
					.AddClient<Timestamped<PlayerTransformsMessage>>(1, DeliveryMethod.Sequenced, H3Server.OnPlayerMove)
					// 
					// =======
					// Server
					// =======
					// Time synchronization
					.AddServer<Timestamped<PingMessage>>(0, DeliveryMethod.Sequenced, H3Client.OnServerPong)
					// Party management
					.AddServer<PartyInitMessage>(1, DeliveryMethod.ReliableOrdered, H3Client.OnServerInit)
					.AddServer<PartyChangeMessage>(1, DeliveryMethod.ReliableOrdered, H3Client.OnServerPartyChange)
					// Asset management
					.AddServer<LevelChangeMessage>(2, DeliveryMethod.ReliableOrdered, H3Client.OnLevelChange)
					.AddServer<PlayerJoinMessage>(2, DeliveryMethod.ReliableOrdered, H3Client.OnPlayerJoin)
					.AddServer<PlayerLeaveMessage>(2, DeliveryMethod.ReliableOrdered, H3Client.OnPlayerLeave)
					// Player movement
					.AddServer<PlayerMovesMessage>(3, DeliveryMethod.Sequenced, H3Client.OnPlayersMove)
				;	
			}
		}

        private void DiscordCallbackHandler(Result result)
        {
			if (result == Result.Ok)
			{
				return;
			}

			Debug.LogError($"Discord activity update failed ({result})");
        }

        private void OnJoin(string rawSecret)
        {
            const string errorPrefix = "Failed to handle join event: ";

            _discordLog.LogDebug($"Received Discord join secret \"{rawSecret}\"");

            bool success = JoinSecret.TryParse(rawSecret, out var secret, out var version);
            if (!_version.CompatibleWith(version))
            {
                _discordLog.LogError(errorPrefix + $"version incompatibility detected (you: {_version}; host: {version})");
                return;
            }

            if (!success)
            {
                _discordLog.LogError(errorPrefix + $"failed to parse join secret \"{rawSecret}\"");
                return;
            }

			ConnectRemote(secret);
        }

        private void OnJoinRequested(ref User user)
        {
            // All friends can join
            // TODO: Change this.
            ActivityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, DiscordCallbackHandler);
        }

        private IEnumerator _HostUnsafe()
		{
			var config = _config.Host;

			var binding = config.Binding;
			var ipv4 = binding.IPv4.Value;
			var ipv6 = binding.IPv6.Value;
			var port = binding.Port.Value;
			var localhost = new IPEndPoint(ipv4 == IPAddress.Any ? IPAddress.Loopback : ipv4, port);

			IPEndPoint publicEndPoint;
			{
				IPAddress publicAddress;
				{
					var getter = config.Public.GetAddress();
					foreach (object o in getter._Run()) yield return o;

					var result = getter.Result;
					
					if (!result.Key) 
					{
						_serverLog.LogFatal($"Failed to get public IP address to host server with: {result.Value}");
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

			Server = new H3Server(_serverLog, _rng, _messages.Server, _messages.ChannelsCount, _version, _config.Host, publicEndPoint);
			_serverLog.LogInfo($"Now hosting on {publicEndPoint}!");

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

		private void Connect(IPEndPoint endPoint, Key32 key, bool isHost, OnH3ClientDisconnect onDisconnect)
        {
			_clientLog.LogInfo($"Connecting to {endPoint}...");
            Client = new H3Client(_clientLog, Activity, _messages.Client, _messages.ChannelsCount, _version, isHost, endPoint, new ConnectionRequestMessage(key), onDisconnect);
        }

		private void ConnectLocal(IPEndPoint endPoint, Key32 key)
		{
			Connect(endPoint, key, true, info => 
			{
				_clientLog.LogError("Disconnected from local server. Something probably caused the frame to hang for more than 5s (debugging breakpoint?). Restarting host...");
				
				StartCoroutine(_Host());
			});
		}

		private void ConnectRemote(JoinSecret secret)
		{
			Client?.Dispose();
			Connect(secret.EndPoint, secret.Key, false, info => 
			{
				_clientLog.LogError("Disconnected from remote server.");
				_clientLog.LogDebug("Suiciding...");

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
			Client?.RenderUpdate();
		}

		private void FixedUpdate()
		{
			DiscordClient.RunCallbacks();

			Client?.Update();
			Server?.Update();
		}

		private void OnDestroy()
		{
			DiscordClient.Dispose();

			Server?.Dispose();
			Client?.Dispose();
		}
    }
}
