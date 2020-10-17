using BepInEx.Configuration;

namespace H3MP
{
	public class HostConfig
	{
		public ConfigEntry<byte> PlayerLimit { get; }

		public HostBindingConfig Binding { get; }

		public HostPublicConfig Public { get; }

		public HostConfig(ConfigFile config, string section)
		{
			PlayerLimit = config.Bind(section, nameof(PlayerLimit), (byte) 4, "The amount of players (including yourself) allowed in a party.");

			Binding = new HostBindingConfig(config, section + "." + nameof(Binding));
			Public = new HostPublicConfig(config, section + "." + nameof(Public));
		}
	}
}