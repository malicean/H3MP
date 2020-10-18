using BepInEx.Logging;
using System;
using System.Collections.Generic;
using H3MP.Messages;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace H3MP.HarmonyPatches
{
	internal class HarmonyState
	{
        public static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Plugin.NAME + "-HM");

		public static string CurrentLevel { get; set; }

        public static bool LockLoadLevel { get; set; } = true;
	}
}