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

		private readonly StatefulActivity _discordActivity;

		public PartyPrivacy Privacy { get; private set; }

		public string Text => PrivacyLocale(Privacy);

		public PrivacyManager(StatefulActivity activity, HostConfig config)
		{
			_discordActivity = activity;

			Privacy = config.PartyPrivacy.Value;
		}

		public void CyclePrivacy()
		{
			if (++Privacy > PARTYPRIVACY_LAST)
			{
				Privacy = PARTYPRIVACY_FIRST;
			}

			_discordActivity.Update(x =>
			{
				x.State = Text;

				return x;
			});
		}
	}
}
