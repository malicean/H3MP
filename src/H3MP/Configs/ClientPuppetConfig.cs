using BepInEx.Configuration;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetConfig
	{
		private const float DEFAULT_HAND_SIZE = 1f;
		private const float DEFAULT_HAND_COLOR = 0.96f;

		public ConfigEntry<Vector3> RootScale { get; }

		public ClientPuppetLimbConfig Head { get; }

		public ClientPuppetLimbConfig HandLeft { get; }
		public ClientPuppetLimbConfig HandRight { get; }

		public ClientPuppetConfig(ConfigFile config, string section)
		{
			RootScale = config.Bind(section, nameof(RootScale), Vector3.one, "The scale of the entire puppet.");

			Head = new ClientPuppetLimbConfig(config, section + "." + nameof(Head), 1f, 0.081f);
			HandLeft = new ClientPuppetLimbConfig(config, section + "." + nameof(HandLeft), DEFAULT_HAND_SIZE, DEFAULT_HAND_COLOR);
			HandRight = new ClientPuppetLimbConfig(config, section + "." + nameof(HandRight), DEFAULT_HAND_SIZE, DEFAULT_HAND_COLOR);
		}
	}
}
