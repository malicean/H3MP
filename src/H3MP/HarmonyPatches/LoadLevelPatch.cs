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
		private static string _currentName;

		public static string CurrentName => _currentName ?? (_currentName = SceneManager.GetActiveScene().name);


		private static void Prefix(string levelName) 
		{
			Plugin.Instance.HarmonyLogger.LogDebug($"Loading {levelName ?? "NULL"}...");

			_currentName = levelName;
			Plugin.Instance.Server?.Broadcast(new LevelChangeMessage(levelName));
		}
	}
}