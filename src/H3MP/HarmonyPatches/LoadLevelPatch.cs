using H3MP.Utils;
using HarmonyLib;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(SteamVR_LoadLevel), nameof(SteamVR_LoadLevel.Begin))]
	internal class LoadLevelPatch
	{
		private static bool Prefix(string levelName)
		{
			var log = HarmonyState.Log.Common;

			if (Plugin.Instance.Peers.Client.MatchSome(out var client) && HarmonyState.LockLoadLevel)
			{
				log.LogDebug($"Blocking level load ({levelName}) and queueing request to server...");
				client.LocalSnapshot.Level = Option.Some(levelName);

				return false;
			}

			log.LogDebug($"Level load triggered: {levelName}");

			HarmonyState.CurrentLevel = levelName;
			return true;
		}
	}
}
