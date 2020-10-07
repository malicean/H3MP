namespace H3MP.Server
{
    public class Settings : IConnectionSettings
	{
		public ushort Port { get; set; } = 7777;

		public bool Allowed { get; set; } = true;

        public int MaxPeers { get; set; } = 4;

		public string Passphrase { get; set; } = string.Empty;

		public string Scene { get; set; } = "MainMenu3";
    }
}
