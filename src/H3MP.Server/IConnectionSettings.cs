namespace H3MP.Server
{
    public interface IConnectionSettings
    {
        public bool Allowed { get; }

        public int MaxPeers { get; }

        public string? Passphrase { get; }
    }
}
