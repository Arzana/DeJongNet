namespace DeJong.Networking.Core.Peers
{
    using Messages;
    using System.Net;

    public sealed class NetClient : Peer
    {
        public NetClient(PeerConfig config)
            : base(config)
        { }

        public void DiscoverRemote(IPEndPoint host)
        {
            AddConnection(host);
        }

        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg) { }
    }
}