namespace DeJong.Networking.Core.Peers
{
    public sealed class NetServer : Peer
    {
        public NetServer(PeerConfig config)
            : base(config)
        {
            CanReceiveConnections = true;
        }
    }
}