using BepInEx;
using BepInEx.Configuration;
using Discord;
using HarmonyLib;
using H3MP.Configs;
using H3MP.HarmonyPatches;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Peers;
using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;
using System.Text;

namespace H3MP
{
	[BepInPlugin(GUID, NAME, VERSION)]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public const string GUID = "Ash.H3MP";
		public const string NAME = "H3MP";
		public const string VERSION = "0.2.0";

		private const long DISCORD_APP_ID = 762557783768956929; // 3rd party RPC application
		private const uint STEAM_APP_ID = 450540; // H3VR

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string path);

		// Unity moment
		public static Plugin Instance { get; private set; }

		private readonly Version _version;
		private readonly RootConfig _config;

		private readonly RandomNumberGenerator _rng;
		private readonly Logs _logs;

		private readonly ActivityManager _activityManager;

		public Discord.Discord GameSDK { get; }

		public StatefulActivity Activity { get; }

		public Server Server { get; private set; }

		public Client Client { get; private set; }

		public Plugin()
		{
			Logger.LogDebug("Binding configs...");
			{
				TomlTypeConverter.AddConverter(typeof(IPAddress), new TypeConverter
				{
					ConvertToObject = (raw, type) => IPAddress.Parse(raw),
					ConvertToString = (value, type) => ((IPAddress) value).ToString()
				});

				_config = new RootConfig(Config);
			}

			Logger.LogDebug("Initializing utilities...");
			{
				_version = new Version(VERSION);

				_rng = RandomNumberGenerator.Create();

				var sensitiveLogging = _config.SensitiveLogging.Value;
				if (sensitiveLogging)
				{
					var warning = @"
┌───────────────────────── SENSITIVE LOGS ENABLED ─────────────────────────┐
│                                                                          │
│ If you do not know what this is or no longer need it, please disable it. │
│        If you do need it enabled, remember to follow the rules:          │
│                                                                          │
|     1. Only share this log file (or console) with people you trust.      │
│     2. Do not share it in a public environment.                          │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘";

					Logger.LogWarning(warning);
				}
				else
				{
					Logger.LogInfo("Sensitive logs will be ignored (this is normal).");
				}

				_logs = new Logs(NAME, sensitiveLogging);
			}

			Logger.LogDebug("Initializing Discord game SDK...");
			{
				// TODO: when BepInEx next releases (>5.3), uncomment this line and move discord_game_sdk.dll to the plugin folder
				// LoadLibrary("BepInEx\\plugins\\H3MP\\" + Discord.Constants.DllName + ".dll");

				GameSDK = new Discord.Discord(DISCORD_APP_ID, (ulong) CreateFlags.Default);
				GameSDK.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
				{
					var log = _logs.Discord;
					switch (level)
					{
						case Discord.LogLevel.Error:
							log.LogError(message);
							break;
						case Discord.LogLevel.Warn:
							log.LogWarning(message);
							break;
						case Discord.LogLevel.Info:
							log.LogInfo(message);
							break;
						case Discord.LogLevel.Debug:
							log.LogDebug(message);
							break;

						default:
							throw new ArgumentOutOfRangeException(level.ToString());
					}
				});

				_activityManager = GameSDK.GetActivityManager();
				Activity = new StatefulActivity(_activityManager, DiscordCallbackHandler);

				_activityManager.RegisterSteam(STEAM_APP_ID);

				_activityManager.OnActivityJoinRequest += OnJoinRequested;
				_activityManager.OnActivityJoin += OnJoin;
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

			var log = _logs.Discord;

			log.LogDebug($"Received Discord join secret \"{rawSecret}\"");

			byte[] data;
			try
			{
				data = Convert.FromBase64String(rawSecret);
			}
			catch
			{
				log.LogError(errorPrefix + "could not parse base 64 secret.");
				return;
			}

			bool success = JoinSecret.TryParse(data, out var secret, out var version);
			if (!_version.CompatibleWith(version))
			{
				log.LogError(errorPrefix + $"version incompatibility detected (you: {_version}; host: {version})");
				return;
			}

			if (!success)
			{
				log.LogError(errorPrefix + "join secret was malformed.");
				return;
			}

			ConnectRemote(secret);
		}

		private void OnJoinRequested(ref User user)
		{
			// All friends can join
			// TODO: Change this.
			_activityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, DiscordCallbackHandler);
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


			float ups = 1 / Time.fixedDeltaTime; // 90
			double tps = config.TickRate.Value;
			if (tps <= 0)
			{
				_serverLog.LogFatal("The configurable tick rate must be a positive value.");
				yield break;
			}

			if (tps > ups)
			{
				tps = ups;
				_serverLog.LogWarning($"The configurable tick rate ({tps:.00}) is greater than the local fixed update rate ({ups:.00}). The config will be ignored and the fixed update rate will be used instead; running a tick rate higher than your own fixed update rate has no benefits.");
			}

			double tickDeltaTime = 1 / tps;

			Server = new H3Server(_serverLog, _config.Host, _rng, _messages.Server, _messages.ChannelsCount, _version, tickDeltaTime, publicEndPoint);
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

		private void Connect(IPEndPoint endPoint, Key32? hostKey, JoinSecret secret, OnH3ClientDisconnect onDisconnect)
		{
			_clientLog.LogInfo($"Connecting to {endPoint}...");

			float ups = 1 / Time.fixedDeltaTime;
			double tps = 1 / secret.TickDeltaTime;

			_clientLog.LogDebug($"Fixed update rate: {ups:.00} u/s");
			_clientLog.LogDebug($"Tick rate: {tps:.00} t/s");

			var request = new ConnectionRequestMessage(secret.Key, hostKey);
			Client = new H3Client(_clientLog, _config.Client, Activity, _messages.Client, _messages.ChannelsCount, secret.TickDeltaTime, _version, endPoint, request, onDisconnect);
		}

		private void ConnectLocal(IPEndPoint endPoint, JoinSecret secret, Key32 hostKey)
		{
			Connect(endPoint, hostKey, secret, info =>
			{
				_clientLog.LogError("Disconnected from local server. Something probably caused the frame to hang for more than 5s (debugging breakpoint?). Restarting host...");

				StartCoroutine(_Host());
			});
		}

		private void ConnectRemote(JoinSecret secret)
		{
			Client?.Dispose();

			Connect(secret.EndPoint, null, secret, info =>
			{
				_clientLog.LogError("Disconnected from remote server.");

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
			GameSDK.RunCallbacks();

			Client?.Update();
			Server?.Update();
		}

		private void OnDestroy()
		{
			GameSDK.Dispose();

			Server?.Dispose();
			Client?.Dispose();
		}
	}
}
