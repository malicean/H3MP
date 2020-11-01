using BepInEx.Configuration;
using H3MP.Models;
using System.Net;

namespace H3MP.Configs
{
	public class HostBindingConfig
	{
		public ConfigEntry<IPAddress> IPv4 { get; }
		public ConfigEntry<IPAddress> IPv6 { get; }
		public ConfigEntry<ushort> Port { get; }

		public ListenBinding Structured => new ListenBinding(IPv4.Value, IPv6.Value, Port.Value);

		public HostBindingConfig(ConfigFile config, string section)
		{
			IPv4 = config.Bind(section, nameof(IPv4), IPAddress.Any, "The IPv4 address to bind the server to. Do not change this unless you have multiple interfaces.");
			IPv6 = config.Bind(section, nameof(IPv6), IPAddress.IPv6Any, "The IPv6 address to bind the server to. Do not change this unless you have multiple interfaces.");
			Port = config.Bind(section, nameof(Port), (ushort) 7777, "The port (UDP only) that the server should listen to.");
		}
	}
}
