using BepInEx.Configuration;
using UnityEngine;

namespace H3MP.Configs
{
	public class ClientPuppetConfig
	{
		public ConfigEntry<Vector3> RootScale { get; }

		public ClientPuppetLimbConfig Head { get; }

		public ClientPuppetLimbConfig HandLeft { get; }
		public ClientPuppetLimbConfig HandRight { get; }

		public ClientPuppetConfig(ConfigFile config, string section)
		{
			RootScale = config.Bind(section, nameof(RootScale), Vector3.one, "The scale of the entire puppet.");

			Head = new ClientPuppetLimbConfig(config, section + "." + nameof(Head), 0.3f, Color.white);
			HandLeft = new ClientPuppetLimbConfig(config, section + "." + nameof(HandLeft), 0.15f, Color.red);
			HandRight = new ClientPuppetLimbConfig(config, section + "." + nameof(HandRight), 0.15f, Color.blue);
		}
	}
}
