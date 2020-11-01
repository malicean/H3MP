namespace H3MP
{
	public class Logs
	{
		public readonly PeerLogs Peers;
		public readonly DiscordLogs Discord;
		public readonly Log Harmony;

		public Logs(string name, bool sensitive)
		{
			const string delimiter = Log.DELIMITER;

			Peers = new PeerLogs(name + delimiter + "PEERS", sensitive);
			Discord = new DiscordLogs(name + delimiter + "DISCORD", sensitive);
			Harmony = new Log(name + delimiter + "HARMONY", sensitive);
		}
	}

	public class DiscordLogs
	{
		public readonly Log Manager;
		public readonly Log SDK;

		public DiscordLogs(string name, bool sensitive)
		{
			const string delimiter = Log.DELIMITER;

			Manager = new Log(name + delimiter + "MANAGER", sensitive);
			SDK = new Log(name + delimiter + "SDK", sensitive);
		}
	}

	public class PeerLogs
	{
		public readonly Log Manager;
		public readonly Log Client;
		public readonly Log Server;

		public PeerLogs(string name, bool sensitive)
		{
			const string delimiter = Log.DELIMITER;

			Manager = new Log(name + delimiter + "MANAGER", sensitive);
			Client = new Log(name + delimiter + "CLIENT", sensitive);
			Server = new Log(name + delimiter + "SERVER", sensitive);
		}
	}
}
