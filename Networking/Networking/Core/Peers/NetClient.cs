namespace DeJong.Networking.Core.Peers
{
    public sealed class NetClient : Peer
    {
        public NetClient(PeerConfig config)
            : base(config)
        {
            CanReceiveConnections = false;
        }
    }
}