using BepInEx.Configuration;
using System;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetLimbConfig
	{
		public ConfigEntry<Vector3> Scale { get; }

		public ClientPuppetLimbColorConfig Color { get; }

		public ClientPuppetLimbConfig(ConfigFile config, string section, float defaultSize, float defaultHue, float defaultValue)
		{
			Scale = config.Bind(section, nameof(Scale), defaultSize * Vector3.one, "The scale of the limb.");

			Color = new ClientPuppetLimbColorConfig(config, section, defaultHue, defaultValue);
		}
	}
}
