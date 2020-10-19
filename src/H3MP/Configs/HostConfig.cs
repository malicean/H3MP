using BepInEx.Configuration;

namespace H3MP.Configs
{
	public class HostConfig
	{
		public ConfigEntry<byte> PlayerLimit { get; }

		public ConfigEntry<double> TickRate { get; }

		public HostBindingConfig Binding { get; }

		public HostPublicBindingConfig PublicBinding { get; }

		public HostPermissionConfig Permissions { get; }

		public HostConfig(ConfigFile config, string section)
		{
			PlayerLimit = config.Bind(section, nameof(PlayerLimit), (byte) 16, "The amount of players (including yourself) allowed in a party. The **theoretical** limit is 255.");
			TickRate = config.Bind(section, nameof(TickRate), 20.0, "The rate (per second) that the network updates. This should be less than or equal to your HMD's refresh rate.");

			Binding = new HostBindingConfig(config, section + "." + nameof(Binding));
			PublicBinding = new HostPublicBindingConfig(config, section + "." + nameof(PublicBinding));
			Permissions = new HostPermissionConfig(config, section + "." + nameof(Permissions));
		}
	}
}
