using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using H3MP.Extensions;
using H3MP.IO;
using H3MP.Messages;
using H3MP.Models;
using H3MP.Peers;
using H3MP.Serialization;
using H3MP.Timing;
using LiteNetLib.Utils;

namespace H3MP
{
	public class DiscordManager : IFixedUpdatable, IDisposable
	{
		private const long DISCORD_APP_ID = 762557783768956929; // 3rd party RPC application
		private const uint STEAM_APP_ID = 450540; // H3VR

		// ONLY add scenes if they have a Discord rich presence asset ("scene_" + scene name).
		private static readonly Dictionary<string, string> _sceneNames = new Dictionary<string, string>
		{
			["MainMenu3"] = "Main Menu",
			["ArizonaTargets"] = "Arizona Range",
			["ArizonaTargets_Night"] = "Arizona at Night",
			["BreachAndClear_TestScene1"] = "Breaching Proto",
			["Cappocolosseum"] = "Cappocolosseum",
			["GrenadeSkeeball"] = "Boomskee",
			["HickockRange"] = "Friendly 45",
			["IndoorRange"] = "Indoor Range",
			["MeatGrinder"] = "Meat Grinder",
			["MeatGrinder_StartingScene"] = "Starting Meat Grinder",
			["MF2_MainScene"] = "Meat Fortress 2",
			["ObstacleCourseScene1"] = "The Gunnasium",
			["ObstacleCourseScene2"] = "Arena Proto",
			["OmnisequencerTesting3"] = "M.E.A.T.S.",
			["ProvingGround"] = "Proving Grounds",
			["SniperRange"] = "Sniper Range",
			["ReturnOfTheRotwieners"] = "Return of the Rotwieners",
			["RotWienersStagingScene"] = "Starting Return of the Rotwieners",
			["SamplerPlatter"] = "Sampler Platter",
			["TakeAndHold_1"] = "Take & Hold: Containment",
			["TakeAndHold_Lobby_2"] = "Take & Hold Lobby",
			["TakeAndHoldClassic"] = "Take & Hold",
			["TakeAndHold_WarIsForWieners"] = "Take & Hold - War is for Wieners",
			["Testing3_LaserSword"] = "Arcade Proto",
			["TileSetTest1"] = "Mini-Arena",
			["TileSetTest_BigHallPerfTest"] = "Take & Hold (Legacy)",
			["WarehouseRange_Rebuilt"] = "Warehouse Range",
			["Wurstwurld1"] = "Wurstwurld",
			["Xmas"] = "Meatmas Snowglobe"
		};

		private readonly DiscordLogs _logs;
		private readonly Version _version;

		private readonly ActivityManager _activityManager;

		public Discord.Discord GameSDK { get; }

		private Activity _activity;
		public Activity Activity
		{
			get => _activity;
			set
			{
				_activityManager.UpdateActivity(value, DiscordCallbackHandler);
				_activity = value;
			}
		}

		public event Action<JoinSecret> Joined;

		public DiscordManager(DiscordLogs logs, Version version)
		{
			_logs = logs;
			_version = version;

			GameSDK = new Discord.Discord(DISCORD_APP_ID, (ulong) CreateFlags.Default);
			GameSDK.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
			{
				var log = _logs.SDK.Common;
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
			_activityManager.RegisterSteam(STEAM_APP_ID);

			_activityManager.OnActivityJoinRequest += OnJoinRequested;
			_activityManager.OnActivityJoin += OnJoin;
		}

		private static void DiscordCallbackHandler(Result result)
		{
		}

		private void OnJoin(string rawSecret)
		{
			var log = _logs.Manager;

			if (log.Sensitive.MatchSome(out var sensitiveLog))
			{
				sensitiveLog.LogDebug($"Received Discord join secret: \"{rawSecret}\"");
			}
			else
			{
				log.Common.LogDebug("Received Discord join secret");
			}

			byte[] data;
			try
			{
				data = Convert.FromBase64String(rawSecret);
			}
			catch
			{
				log.Common.LogError("Could not parse base 64 join secret.");
				return;
			}

			var netData = new NetDataReader(data);
			var reader = new BitPackReader(netData);
			var serializer = CustomSerializers.JoinSecret;

			var version = serializer.DeserializeVersion(ref reader);
			if (!_version.CompatibleWith(version))
			{
				log.Common.LogError($"Version incompatibility detected (you: {_version}; host: {version})");
				return;
			}

			JoinSecret secret;
			try
			{
				secret = serializer.ContinueDeserialize(ref reader, version);
			}
			catch
			{
				log.Common.LogError("Join secret was malformed.");
				return;
			}

			Joined?.Invoke(secret);
		}

		private void OnJoinRequested(ref User user)
		{
			// All friends can join
			// TODO: Change this.
			_activityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, DiscordCallbackHandler);
		}

		private static string ToBase64<TValue>(TValue value, ISerializer<TValue> serializer)
		{
			var writer = new NetDataWriter();
			var bits = new BitPackWriter(writer);

			serializer.Serialize(ref bits, value);
			bits.Dispose();

			return Convert.ToBase64String(writer.Data, 0, writer.Length);
		}

		public void HandleWorldDelta(Client client, DeltaWorldSnapshotMessage delta)
		{
			var activity = Activity;
			var dirty = false;

			if (delta.PartyID.MatchSome(out var partyID))
			{
				dirty = true;

				activity.Party.Id = ToBase64(partyID, CustomSerializers.Key32);
			}

			if (delta.Secret.MatchSome(out var secret))
			{
				dirty = true;

				activity.Secrets.Join = ToBase64(secret, CustomSerializers.JoinSecret);
			}

			if (delta.Level.MatchSome(out var level))
			{
				dirty = true;

				activity.Timestamps.Start = DateTime.UtcNow.ToUnixTimestamp();

				string asset;
				if (_sceneNames.TryGetValue(level, out var tooltip))
				{
					asset = level.ToLower();
				}
				else
				{
					asset = "unknown";
					tooltip = level;
				}

				activity.Assets.LargeImage = "scene_" + asset;
				activity.Assets.LargeText = tooltip;
			}

			if (delta.PlayerBodies.MatchSome(out var players))
			{
				var playerCount = players.Count(x => x.MatchSome(out var innerDelta) && innerDelta.IsSome);
				ref var partySize = ref activity.Party.Size.CurrentSize;

				if (playerCount > 0 && partySize != playerCount)
				{
					dirty = true;

					activity.State = "In a party";
					activity.Party.Size.MaxSize = client.MaxPlayers;

					partySize = playerCount;
				}
			}

			if (dirty)
			{
				Activity = activity;
			}
		}

		public void FixedUpdate()
		{
			GameSDK.RunCallbacks();
		}

		public void Dispose()
		{
			GameSDK.Dispose();
		}
	}
}
