using BepInEx.Logging;

namespace H3MP
{
	public class Logs
	{
		public readonly Log Client;
		public readonly Log Server;
		public readonly Log Discord;
		public readonly Log Harmony;

		public Logs(string name, bool sensitive)
		{
			const string delimiter = Log.DELIMITER;

			Client = new Log(name + delimiter + "CL", sensitive);
			Server = new Log(name + delimiter + "SV", sensitive);
			Discord = new Log(name + delimiter + "DC", sensitive);
			Harmony = new Log(name + delimiter + "HM", sensitive);
		}
	}
}
