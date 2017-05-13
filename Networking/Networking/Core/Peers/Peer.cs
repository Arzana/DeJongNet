namespace DeJong.Networking.Core.Peers
{
    public class Peer
    {
        public PeerConfig Config { get; private set; }

        private RawSocket socket;
        private Connection[] connections;
    }
}