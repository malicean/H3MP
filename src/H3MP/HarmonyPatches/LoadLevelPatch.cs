using System;
using System.Collections.Generic;
using H3MP.Messages;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace H3MP.HarmonyPatches
{
	[HarmonyPatch(typeof(SteamVR_LoadLevel), nameof(SteamVR_LoadLevel.Begin))]
	internal class LoadLevelPatch
	{
		private static bool Prefix(string levelName) 
		{
			var plugin = Plugin.Instance;
			var log = HarmonyState.Log;

			if (HarmonyState.LockLoadLevel)
			{
				log.LogDebug($"Blocking level load ({levelName}) and sending request to server...");
				plugin.Client.Server.Send(new LevelChangeMessage(levelName));

				return false;
			}
			else
			{
				log.LogDebug($"Level load triggered: {levelName}");
			}

			HarmonyState.CurrentLevel = levelName;
			return true;
		}
	}
}