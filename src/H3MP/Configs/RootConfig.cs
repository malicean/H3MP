using BepInEx.Configuration;

namespace H3MP.Configs
{
	public class RootConfig
	{
		private const string ROOT_SECTION = ".";

		public ConfigEntry<bool> AutoHost { get; }

		public ClientConfig Client { get; }

		public HostConfig Host { get; }

		public RootConfig(ConfigFile config)
		{
			AutoHost = config.Bind(ROOT_SECTION, nameof(AutoHost), true, "Automatically host a party when you are not connected to another party.");

			Client = new ClientConfig(config, nameof(Client));
			Host = new HostConfig(config, nameof(Host));
		}
	}
}
