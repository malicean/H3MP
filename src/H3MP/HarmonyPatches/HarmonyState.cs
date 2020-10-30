using System;
using BepInEx.Logging;
using UnityEngine.SceneManagement;

namespace H3MP.HarmonyPatches
{
	internal class HarmonyState
	{
		public static Log Log { get; private set; }

		private static string _currentLevel;
		public static string CurrentLevel
		{
			get => _currentLevel ?? (_currentLevel = SceneManager.GetActiveScene().name);
			set => _currentLevel = value;
		}

		public static bool LockLoadLevel { get; set; } = true;

		public static void Init(Log log)
		{
			if (Log != null)
			{
				throw new InvalidOperationException("Already initialized.");
			}

			Log = log;
		}
	}
}
