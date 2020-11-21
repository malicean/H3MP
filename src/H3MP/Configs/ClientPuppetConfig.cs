using BepInEx.Configuration;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetConfig
	{
		private const float DEFAULT_HAND_COLOR_HUE = 0.96f;
		private const float DEFAULT_HAND_COLOR_VALUE = 1f;

		public ClientPuppetLimbConfig HandLeft { get; }
		public ClientPuppetLimbConfig HandRight { get; }

		public ClientPuppetConfig(ConfigFile config, string section)
		{
			HandLeft = new ClientPuppetLimbConfig(config, section + "." + nameof(HandLeft), DEFAULT_HAND_COLOR_HUE, DEFAULT_HAND_COLOR_VALUE);
			HandRight = new ClientPuppetLimbConfig(config, section + "." + nameof(HandRight), DEFAULT_HAND_COLOR_HUE, DEFAULT_HAND_COLOR_VALUE);
		}
	}
}
