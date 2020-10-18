using BepInEx.Configuration;

namespace H3MP
{
	public class HostConfig
	{
		public ConfigEntry<byte> PlayerLimit { get; }

		public HostBindingConfig Binding { get; }

		public HostPublicBindingConfig PublicBinding { get; }

		public HostPermissionConfig Permissions { get; }

		public HostConfig(ConfigFile config, string section)
		{
			PlayerLimit = config.Bind(section, nameof(PlayerLimit), (byte) 4, "The amount of players (including yourself) allowed in a party. The **theoretical** limit is 255.");

			Binding = new HostBindingConfig(config, section + "." + nameof(Binding));
			PublicBinding = new HostPublicBindingConfig(config, section + "." + nameof(PublicBinding));
			Permissions = new HostPermissionConfig(config, section + "." + nameof(Permissions));
		}
	}
}