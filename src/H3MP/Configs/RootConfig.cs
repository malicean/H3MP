using BepInEx.Configuration;

namespace H3MP
{
    public class RootConfig
    {
        public ConfigEntry<bool> AutoHost { get; }

        public HostConfig Host { get; }

        public RootConfig(ConfigFile config, string section)
        {
            AutoHost = config.Bind(section, nameof(AutoHost), true, "Automatically host a party when you are not connected to another party.");

            Host = new HostConfig(config, section + "." + nameof(Host));
        }
    }
}