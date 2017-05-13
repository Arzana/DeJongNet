namespace DeJong.Networking.Core.Peers
{
    public enum PeerStatus : byte
    {
        NotRunning,
        Starting,
        Running,
        ShutdownRequested
    }
}