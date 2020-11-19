using BepInEx.Logging;
using FistVR;
using H3MP.Messages;
using H3MP.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace H3MP.HarmonyPatches
{
	internal class HarmonyState
	{
		public static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource(Plugin.NAME + "-HM");

		public static StatefulActivity DiscordActivity { get; private set; }

		public static WristMenuButtons WristMenuButtons { get; private set; }

		private static string _currentLevel;
		public static string CurrentLevel
		{
			get => _currentLevel ?? (_currentLevel = SceneManager.GetActiveScene().name);
			set => _currentLevel = value;
		}

		public static bool LockLoadLevel { get; set; } = true;

		public static void Init(StatefulActivity discordActivity, WristMenuButtons wristmenubuttons)
		{
			DiscordActivity = discordActivity;
			WristMenuButtons = wristmenubuttons;
		}
	}
}
