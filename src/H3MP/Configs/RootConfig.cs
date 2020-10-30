using BepInEx.Configuration;

namespace H3MP.Configs
{
	public class RootConfig
	{
		private const string ROOT_SECTION = ".";

		public ConfigEntry<bool> AutoHost { get; }

		public ConfigEntry<bool> SensitiveLogging { get; }

		public ClientConfig Client { get; }

		public HostConfig Host { get; }

		public RootConfig(ConfigFile config)
		{
			AutoHost = config.Bind(ROOT_SECTION, nameof(AutoHost), true, "Automatically host a party when you are not connected to another party.");
			SensitiveLogging = config.Bind(ROOT_SECTION, nameof(SensitiveLogging), false, "ONLY ENABLE THIS IF YOU HAVE BEEN ASKED TO OR ARE DEBUGGING, and read the implications: Whether or not to show information that users (you and others) may not want other people to see. When this is enabled, you should 1) only share logs with people you trust, and 2) do not post them in public locations.");

			Client = new ClientConfig(config, nameof(Client));
			Host = new HostConfig(config,  nameof(Host));
		}
	}
}
