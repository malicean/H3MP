using Discord;
using H3MP.Extensions;
using H3MP.Messages;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(SteamVR_LoadLevel), nameof(SteamVR_LoadLevel.Begin))]
	internal class LoadLevelPatch
	{
		// ONLY add scenes if they have a Discord rich presence asset ("scene_" + scene name).
		private static readonly Dictionary<string, string> _sceneNames = new Dictionary<string, string>
		{
			["MainMenu3"] = "Main Menu",
			["ArizonaTargets"] = "Arizona Range",
			["ArizonaTargets_Night"] = "Arizona at Night",
			["BreachAndClear_TestScene1"] = "Breaching Proto",
			["Cappocolosseum"] = "Cappocolosseum",
			["GrenadeSkeeball"] = "Boomskee",
			["HickockRange"] = "Friendly 45 Range",
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
			["WinterWasteland"] = "Winter Wasteland",
			["Wurstwurld1"] = "Wurstwurld",
			["Xmas"] = "Meatmas Snowglobe"
		};

		private static void SetActivity(string levelName)
		{
			string asset;
			if (_sceneNames.TryGetValue(levelName, out var tooltip))
			{
				asset = levelName.ToLower();
			}
			else
			{
				asset = "unknown";
				tooltip = levelName;
			}

			HarmonyState.DiscordActivity.Update(x =>
			{
				x.Assets = new ActivityAssets
				{
					LargeImage = "scene_" + asset,
					LargeText = tooltip
				};
				x.Timestamps = new Discord.ActivityTimestamps
				{
					Start = DateTime.UtcNow.ToUnixTimestamp()
				};

				return x;
			});
		}

		private static bool Prefix(string levelName)
		{
			var plugin = Plugin.Instance;
			var log = HarmonyState.Log;
			var client = plugin.Client;

			if (!(client is null) && HarmonyState.LockLoadLevel)
			{
				log.LogDebug($"Blocking level load ({levelName}) and sending request to server...");
				client.Server.Send(new LevelChangeMessage(levelName));

				return false;
			}
			else
			{
				log.LogDebug($"Level load triggered: {levelName}");
			}

			SetActivity(levelName);

			HarmonyState.CurrentLevel = levelName;
			return true;
		}
	}
}
