using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;

namespace H3MP.Configs
{
	public class HostPublicBindingConfig
	{
		private const string ADDRESS_AUTO = "auto";
		private const string ADDRESS_AUTO_V4 = ADDRESS_AUTO + ".v4";
		private const string ADDRESS_AUTO_V6 = ADDRESS_AUTO + ".v6";

		// HTTPS is pointless: the user's public IP is the only content, but it can also be found in the headers of packets (which are not protected by HTTPS for obvious reasons).
		private const string ADDRESS_AUTO_V4_URL = "http://ipv4.icanhazip.com";
		private const string ADDRESS_AUTO_V6_URL = "http://ipv6.icanhazip.com";

		public ConfigEntry<string> Address { get; }

		public ConfigEntry<ushort> Port { get; }

		public HostPublicBindingConfig(ConfigFile config, string section)
		{
			Address = config.Bind(section, nameof(Address), ADDRESS_AUTO_V4,
				"The IP address that clients should use to connect to the server. " +
				"If set to \"" + ADDRESS_AUTO_V4 + "\" this will be the value from " + ADDRESS_AUTO_V4_URL + ", whereas \"" + ADDRESS_AUTO_V6 + "\" will use " + ADDRESS_AUTO_V6_URL + ". " +
				"You can also set it the config to an IP address and it will use it (manual mode) instead of automatic mode. " +
				"For more info about the website used to obtain your IP address automatically, visit https://major.io/icanhazip-com-faq/.");
			Port = config.Bind(section, nameof(Port), (ushort) 0,
				"The port that clients should use to connect to the server. " +
				"If set to 0, this will be the same as the binding port. " +
				"If you are in a subnet and the forward-from port does not match the forward-to port, this MUST be changed to the forward-from port or clients will not be able to connect.");
		}

		public AddressGetter GetAddress()
		{
			return new AddressGetter(Address.Value);
		}

		public class AddressGetter
		{
			private string _config;

			public AddressGetter(string configValue)
			{
				_config = configValue;
			}

			// Tuples weren't invented yet.
			public KeyValuePair<bool, string> Result { get; private set; }

			public IEnumerable _Run()
			{
				string url;
				switch (_config)
				{
					case ADDRESS_AUTO_V4:
						url = ADDRESS_AUTO_V4_URL;
						break;

					case ADDRESS_AUTO_V6:
						url = ADDRESS_AUTO_V6_URL;
						break;

					default:
						Result = IPAddress.TryParse(_config, out _)
							? new KeyValuePair<bool, string>(true, _config)
							: new KeyValuePair<bool, string>(false, "manual; Invalid IP address in config.");
						yield break;
				}

				var request = UnityWebRequest.Get(url);
				yield return request.Send();

				if (request.isError)
				{
					Result = new KeyValuePair<bool, string>(false, "automatic; " + request.error);
					yield break;
				}

				Result = new KeyValuePair<bool, string>(true, request.downloadHandler.text.Trim());
			}
		}
	}
}
