using BepInEx.Configuration;
using System;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetLimbConfig
	{
		public ClientPuppetLimbColorConfig Color { get; }

		public ClientPuppetLimbConfig(ConfigFile config, string section, float defaultHue, float defaultValue)
		{
			Color = new ClientPuppetLimbColorConfig(config, section, defaultHue, defaultValue);
		}
	}
}
