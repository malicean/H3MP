using BepInEx.Configuration;
using System;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetLimbColorConfig
	{
		public ConfigEntry<float> Hue { get; }

		public ConfigEntry<float> Value { get; }

		public ClientPuppetLimbColorConfig(ConfigFile config, string section, float defaultHue, float defaultValue)
		{
			Hue = config.Bind(section, nameof(Hue), defaultHue, "The HSV hue color of the limb.");
			Value = config.Bind(section, nameof(Value), defaultValue, "The HSV value color of the limb.");
		}
	}
}
