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
	[BepInPlugin(Plugin.GUID, Plugin.NAME, Plugin.VERSION)]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public const string GUID = "ash.h3mp";
		public const string NAME = "H3MP";
		public const string VERSION = "0.0.0";

		private const long DISCORD_APP_ID = 762557783768956929; // 3rd party RPC application
		private const uint STEAM_APP_ID = 450540; // H3VR

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string path);

		// Unity moment
		public static Plugin Instance { get; private set; }

		private readonly Version _version;

		private readonly RootConfig _config;

		private readonly RandomNumberGenerator _rng;

		private readonly ManualLogSource _clientLog;
		private readonly ManualLogSource _serverLog;
		private readonly ManualLogSource _discordLog;

		private readonly UniversalMessageList<H3Client, H3Server> _messages;

		public Discord.Discord DiscordClient { get; }

		public ActivityManager ActivityManager { get; }

		public StatefulActivity Activity { get; }

		public H3Server Server { get; private set; }

		public H3Client Client { get; private set; }

		public Plugin()
		{
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
				_version = new Version(VERSION);
				_rng = RandomNumberGenerator.Create();

				_clientLog = BepInEx.Logging.Logger.CreateLogSource(NAME + "-CL");
				_serverLog = BepInEx.Logging.Logger.CreateLogSource(NAME + "-SV");
				_discordLog = BepInEx.Logging.Logger.CreateLogSource(NAME + "-DC");
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
					// Time synchronization (reliable adds latency)
					.AddClient<PingMessage>(0, DeliveryMethod.Sequenced, H3Server.OnClientPing)
					// Player movement
					.AddClient<Timestamped<PlayerTransformsMessage>>(1, DeliveryMethod.Sequenced, H3Server.OnPlayerMove)
					// Asset management
					.AddClient<LevelChangeMessage>(2, DeliveryMethod.ReliableOrdered, H3Server.OnLevelChange)
					// 
					// =======
					// Server
					// =======
					// Time synchronization (reliable adds latency)
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

			Logger.LogDebug("Initializing shared Harmony state...");
			HarmonyState.Init(Activity);
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

			byte[] data;
			try
			{
				data = Convert.FromBase64String(rawSecret);
			}
			catch
			{
				_discordLog.LogError(errorPrefix + "could not parse base 64 secret.");
				return;
			}

			bool success = JoinSecret.TryParse(data, out var secret, out var version);
			if (!_version.CompatibleWith(version))
			{
				_discordLog.LogError(errorPrefix + $"version incompatibility detected (you: {_version}; host: {version})");
				return;
			}

			if (!success)
			{
				_discordLog.LogError(errorPrefix + $"join secret was malformed.");
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
			_serverLog.LogDebug("Starting server...");

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
					var getter = config.PublicBinding.GetAddress();
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

				ushort publicPort = config.PublicBinding.Port.Value;
				if (publicPort == 0)
				{
					publicPort = port;
				}

				publicEndPoint = new IPEndPoint(publicAddress, publicPort);
			}


			float ups = 1f / Time.fixedDeltaTime; // 90
			_serverLog.LogDebug($"Fixed update: {ups:.00} u/s");

			float minTps = ups / byte.MaxValue; // ups u/s divided by max u/t
			float userTps = config.TickRate.Value;
			if (userTps <= minTps || ups < userTps)
			{
				_serverLog.LogFatal($"The configurable tick rate must fall within the range [{minTps:.00}, {ups:.00}]. Current value: {userTps:.00}");
				yield break;
			}

			float userUpt = ups / userTps;
			var upt = (byte) Mathf.FloorToInt(userUpt);
			float tps = ups / upt;
			if (Mathf.Abs(upt - userUpt) < float.Epsilon)
			{
				_serverLog.LogWarning($"The configurable tick rate ({userTps:.00} t/s; {userUpt:.00} u/t) is unaligned! It has been rounded up ({tps:.00} t/s; {upt:N0} u/t).");
			}

			_serverLog.LogDebug($"Tick rate: {upt:N0} u/t; {ups / upt:N0} t/s");

			Server = new H3Server(_serverLog, _rng, _messages.Server, _messages.ChannelsCount, _version, upt, _config.Host, publicEndPoint);
			_serverLog.LogInfo($"Now hosting on {publicEndPoint}!");

			ConnectLocal(localhost, Server.Secret, Server.HostKey);
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

		private void Connect(IPEndPoint endPoint, Key32 key, Key32? hostKey, byte upt, OnH3ClientDisconnect onDisconnect)
		{
			_clientLog.LogInfo($"Connecting to {endPoint}...");

			var request = new ConnectionRequestMessage(key, hostKey);
			Client = new H3Client(_clientLog, Activity, _messages.Client, _messages.ChannelsCount, upt, _version, endPoint, request, onDisconnect);
		}

		private void ConnectLocal(IPEndPoint endPoint, JoinSecret secret, Key32 hostKey)
		{
			Connect(endPoint, secret.Key, hostKey, secret.UpdatesPerTick, info => 
			{
				_clientLog.LogError("Disconnected from local server. Something probably caused the frame to hang for more than 5s (debugging breakpoint?). Restarting host...");
				
				StartCoroutine(_Host());
			});
		}

		private void ConnectRemote(JoinSecret secret)
		{
			Client?.Dispose();
			Connect(secret.EndPoint, secret.Key, null, secret.UpdatesPerTick, info => 
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
