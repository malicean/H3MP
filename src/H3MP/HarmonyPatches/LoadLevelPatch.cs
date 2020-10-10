using System;
using System.Collections.Generic;
using H3MP.Messages;
using HarmonyLib;
using UnityEngine;

namespace H3MP.HarmonyPatches
{
    [HarmonyPatch(typeof(SteamVR_LoadLevel), nameof(SteamVR_LoadLevel.Begin))]
    internal class LoadLevelPatch
    {
        private static readonly Dictionary<string, string> _levelNames = new Dictionary<string, string>
        {
            ["MainMenu3"] = "Main Menu",
            ["IndoorRange"] = "Indoor Range"
        };

        public static string CurrentName { get; private set; }

        private static void Prefix(string levelName) 
        {
            CurrentName = levelName;

            if (!_levelNames.TryGetValue(levelName, out var tooltip))
            {
                Debug.LogWarning($"Failed to find localized name for level \"{tooltip}\". Falling back to raw level name.");
                tooltip = levelName;
            }

            var plugin = Plugin.Instance;
            plugin.Discord.UpdateLargeAsset(levelName, tooltip);
            plugin.Server?.Broadcast(new LevelChangeMessage(levelName));
        }
    }
}