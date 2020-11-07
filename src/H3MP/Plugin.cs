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
using System.Diagnostics;
using H3MP.Timing;

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
		private readonly Stopwatch _watch;

		public Version Version { get; }

		public RandomNumberGenerator Random { get; }

		public DiscordManager Discord { get; }

		public PeerManager Peers { get; }

		public RenderFrameClock RenderFrame { get; }

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
				_watch = Stopwatch.StartNew();

				Version = new Version(VERSION);
				Random = RandomNumberGenerator.Create();
			}

			Logger.LogDebug("Initializing peer manager...");
			Peers = new PeerManager(_logs.Peers, _config.Peers, Version, _watch, StartCoroutine);

			Logger.LogDebug("Initializing Harmony...");
			{
				HarmonyState.Init(_logs.Harmony);
				new Harmony(Info.Metadata.GUID).PatchAll();
			}

			Logger.LogDebug("Initializing Discord manager...");
			// TODO: when BepInEx next releases (>5.3), uncomment this line and move discord_game_sdk.dll to the plugin folder
			// LoadLibrary("BepInEx\\plugins\\H3MP\\" + Discord.Constants.DllName + ".dll");
			Discord = new DiscordManager(_logs.Discord, Version);

			RenderFrame = new RenderFrameClock(_watch);

			Puppeteer = Option.None<Puppeteer>();

			Discord.Joined += Joined;
			Peers.ServerCreated += ServerCreated;
			Peers.ClientCreated += ClientCreated;
			Peers.ClientCreated += Discord.HandleClientCreated;
			Peers.ClientKilled += ClientKilled;
		}

		private void Joined(JoinSecret secret)
		{
			Peers.Connect(secret);
		}

		private void ServerCreated(Server server)
		{
			server.Simulate += ServerTick;
		}

		private void ServerTick(Server server)
		{
			var level = Option.None<TickStamped<string>>();
			var perms = _config.Peers.Host.Permissions;

			foreach (var husk in server.ConnectedHusks)
			{
				if (!husk.Input.MatchSome(out var inputValue))
				{
					return;
				}

				var tick = inputValue.Stamp;
				var content = inputValue.Content;

				server.LocalSnapshot.PlayerBodies[husk.ID] = Option.Some(content.Body);

				if (content.Level.MatchSome(out var contentLevel))
				{
					if (!level.MatchSome(out var nextLevel) || tick > nextLevel.Stamp)
					{
						bool hasPerm = husk.IsAdmin;

						if (!hasPerm)
						{
							var perm = contentLevel == server.LocalSnapshot.Level.Name
								? perms.SceneReloading
								: perms.SceneChanging;
							hasPerm = perm.Value;
						}

						if (hasPerm)
						{
							var stamped = new TickStamped<string>(tick, contentLevel);
							level = Option.Some(stamped);
						}
					}
				}
			}

			if (level.Map(x => x.Content).MatchSome(out var levelValue))
			{
				ref var snapshotLevel = ref server.LocalSnapshot.Level;
				snapshotLevel = snapshotLevel.Next(levelValue);
			}
		}

		private void ClientCreated(Client client)
		{
			client.DeltaSnapshotReceived += ClientDelta;
			client.Simulate += ClientSimulate;
			client.Cleanup += ClientCleanup;

			var puppeteer = new Puppeteer(client, RenderFrame);
			Puppeteer = Option.Some(puppeteer);
		}

		private void ClientDelta(Client client, TickTimeStamped<DeltaWorldSnapshotMessage> delta)
		{
			if (delta.Content.Level.MatchSome(out var level))
			{
				HarmonyState.LockLoadLevel = false;

				try
				{
					SteamVR_LoadLevel.Begin(level.Name);
				}
				finally
				{
					HarmonyState.LockLoadLevel = true;
				}
			}
		}

		private void ClientSimulate(Client client)
		{
			var body = GM.CurrentPlayerBody;

			TransformMessage GetTransform(Transform transform)
			{
				return new TransformMessage
				{
					Position = transform.position,
					Rotation = transform.rotation
				};
			}

			client.LocalSnapshot.Body = new BodyMessage
			{
				Head = GetTransform(body.Head),
				HandLeft = GetTransform(body.LeftHand),
				HandRight = GetTransform(body.RightHand)
			};
		}

		private void ClientCleanup(Client client)
		{
			client.LocalSnapshot.Level = Option.None<string>();
		}

		private void ClientKilled(Client obj)
		{
			var activity = Discord.Activity;

			activity.Party = default;
			activity.Secrets = default;

			if (Puppeteer.MatchSome(out var puppeteer))
			{
				puppeteer.Dispose();
				Puppeteer = Option.None<Puppeteer>();
			}
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
			RenderFrame.Update();
		}

		private void OnDestroy()
		{
			Discord.Dispose();
			Peers.Dispose();
		}
	}
}
