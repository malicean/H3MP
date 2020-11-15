using BepInEx.Logging;
using H3MP.Configs;
using H3MP.HarmonyPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H3MP.Utils
{
	public class PrivacyManager
	{
		private readonly ManualLogSource _log;
		private readonly Configs.HostConfig _config;

		public enum PartyPrivacy
		{
			Open,
			Friends,
			InviteOnly,
		}

		private static string PrivacyLocale(PartyPrivacy value)
		{
			switch (value)
			{
				case PartyPrivacy.Open: return "Open Party";
				case PartyPrivacy.Friends: return "Friends Only Party";
				case PartyPrivacy.InviteOnly: return "Invite Only Party";
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private const PartyPrivacy PARTYPRIVACY_FIRST = PartyPrivacy.Open;
		private const PartyPrivacy PARTYPRIVACY_LAST = PartyPrivacy.InviteOnly;

		public static PartyPrivacy Privacy = default;

		public static string PrivacyText { get; set; }

		public PrivacyManager(ManualLogSource log, HostConfig config)
		{
			_log = log;
			_config = config;

			Privacy = config.PartyPrivacy.Value;
			PrivacyText = PrivacyLocale(Privacy);
		}

		public static void CyclePrivacy()
		{
			if (++Privacy > PARTYPRIVACY_LAST)
			{
				Privacy = PARTYPRIVACY_FIRST;
			}
			HarmonyState.DiscordActivity.Update(x =>
			{
				x.State = PrivacyLocale(Privacy);
				return x;
			});
			PrivacyText = PrivacyLocale(Privacy);
		}
	}
}
