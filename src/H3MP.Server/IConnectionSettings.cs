namespace H3MP
{
    public interface IConnectionSettings
    {
        public bool Allowed { get; }

        public int MaxPeers { get; }

        public string? Key { get; }
    }
}