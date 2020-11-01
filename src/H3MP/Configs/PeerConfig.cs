using BepInEx.Configuration;

namespace H3MP.Configs
{
	public class PeerConfig
	{
		public ConfigEntry<bool> AutoHost { get; }

		public ClientConfig Client { get; }

		public HostConfig Host { get; }

		public PeerConfig(ConfigFile config, string section)
		{
			AutoHost = config.Bind(section, nameof(AutoHost), true, "Automatically host a party when you are not connected to another party.");

			Client = new ClientConfig(config, nameof(Client));
			Host = new HostConfig(config,  nameof(Host));
		}
	}
}
