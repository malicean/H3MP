using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using H3MP.Configs;
using H3MP.HarmonyPatches;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using H3MP.Peers;
using H3MP.Utils;
using H3MP.Models;
using H3MP.Messages;
using H3MP.Extensions;
using System.Collections.Generic;

namespace H3MP
{
	[BepInPlugin(GUID, NAME, VERSION)]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public const string GUID = "Ash.H3MP";
		public const string NAME = "H3MP";
		public const string VERSION = "0.2.0";

		// [DllImport("kernel32.dll")]
		// private static extern IntPtr LoadLibrary(string path);

		// Unity moment
		public static Plugin Instance { get; private set; }

		private readonly RootConfig _config;
		private readonly Logs _logs;

		public Version Version { get; }

		public RandomNumberGenerator Random { get; }

		public DiscordManager Discord { get; }

		public PeerManager Peers { get; }

		public Plugin()
		{
			Instance = this;

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

				Version = new Version(VERSION);
				Random = RandomNumberGenerator.Create();
			}

			Logger.LogDebug("Initializing peer manager...");
			Peers = new PeerManager(_logs.Peers, _config.Peers, Version, StartCoroutine);

			Logger.LogDebug("Initializing Harmony...");
			{
				HarmonyState.Init(_logs.Harmony);
				new Harmony(Info.Metadata.GUID).PatchAll();
			}

			Logger.LogDebug("Initializing Discord manager...");
			// TODO: when BepInEx next releases (>5.3), uncomment this line and move discord_game_sdk.dll to the plugin folder
			// LoadLibrary("BepInEx\\plugins\\H3MP\\" + Discord.Constants.DllName + ".dll");
			Discord = new DiscordManager(_logs.Discord, Version);

			Discord.Joined += Joined;

			Peers.ClientCreated += ClientCreated;
			Peers.ClientKilled += ClientKilled;
		}

		private void Joined(JoinSecret secret)
		{
			Peers.Connect(secret);
		}

		private void ClientCreated(Client client)
		{
			client.DeltaSnapshotReceived += (buffer, serverTick, delta) => Discord.HandleWorldDelta(delta);
		}

		private void ClientKilled(Client obj)
		{
			var activity = Discord.Activity;

			activity.Party = default;
			activity.Secrets = default;
		}

		private void Start()
		{
			Peers.Start();
		}

		private void FixedUpdate()
		{
			Discord.FixedUpdate();
			Peers.FixedUpdate();
		}

		private void OnDestroy()
		{
			Discord.Dispose();
			Peers.Dispose();
		}
	}
}
