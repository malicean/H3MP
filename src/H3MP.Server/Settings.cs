namespace H3MP.Server
{
    public class Settings : IConnectionSettings
	{
		public bool Allowed { get; set; } = true;

        public int MaxPeers { get; set; } = 4;

		public string? Key { get; set; } = null;
    }
}
