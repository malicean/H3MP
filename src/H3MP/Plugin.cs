using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using H3MP.Configs;
using H3MP.HarmonyPatches;
using System;
using System.Net;
using System.Security.Cryptography;
using H3MP.Peers;
using H3MP.Utils;
using H3MP.Models;
using H3MP.Messages;
using FistVR;
using UnityEngine;
using H3MP.Puppetting;

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

		public Option<Puppeteer> Puppeteer { get; private set; }

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

			Puppeteer = Option.None<Puppeteer>();

			Discord.Joined += Joined;

			Peers.ServerCreated += ServerCreated;

			Peers.ClientCreated += ClientCreated;
			Peers.ClientKilled += ClientKilled;
		}

		private void Joined(JoinSecret secret)
		{
			Peers.Connect(secret);
		}

		private void ServerCreated(Server server)
		{
			server.Ticked += ServerTick;
		}

		private void ServerTick()
		{
			var server = Peers.Server.Unwrap();

			foreach (var husk in server.ConnectedHusks)
			{
				if (!husk.Input.MatchSome(out var inputValue))
				{
					return;
				}

				server.LocalSnapshot.PlayerBodies[husk.ID] = Option.Some(inputValue.Content.Body);
			}
		}

		private void ClientCreated(Client client)
		{
			client.Ticked += ClientTick;
			client.DeltaSnapshotReceived += (buffer, serverTick, delta) => Discord.HandleWorldDelta(client, delta);

			Puppeteer = Option.Some(new Puppeteer(client));
		}

		private void ClientTick()
		{
			var client = Peers.Client.Unwrap();

			var root = GM.CurrentPlayerRoot;
			var body = GM.CurrentPlayerBody;

			TransformMessage LocalTransform(Transform transform)
			{
				return new TransformMessage
				{
					Position = transform.localPosition,
					Rotation = transform.localRotation
				};
			}

			client.LocalSnapshot.Body = new BodyMessage
			{
				Root = new TransformMessage
				{
					Position = root.position,
					Rotation = root.rotation
				},
				Head = LocalTransform(body.Head),
				HandLeft = LocalTransform(body.LeftHand),
				HandRight = LocalTransform(body.RightHand)
			};
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

		private void Update()
		{
			if (Puppeteer.MatchSome(out var puppeteer))
			{
				puppeteer.RenderUpdate();
			}
		}

		private void OnDestroy()
		{
			Discord.Dispose();
			Peers.Dispose();
		}
	}
}
