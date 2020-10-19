using BepInEx.Configuration;

namespace H3MP.Configs
{
	public class HostPermissionConfig
	{
		public ConfigEntry<bool> SceneChanging { get; }

		public ConfigEntry<bool> SceneReloading { get; }

		public HostPermissionConfig(ConfigFile config, string section)
		{
			SceneChanging = config.Bind(section, nameof(SceneChanging), true, "Whether or not external clients can change the scene.");
			SceneReloading = config.Bind(section, nameof(SceneReloading), true, "Whether or not external clients can reload the scene.");
		}
	}
}
