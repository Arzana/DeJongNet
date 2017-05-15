using System.Collections.Generic;

namespace DeJong.Networking.Core.Peers
{
    public abstract class Peer
    {
        public NetID ID { get; private set; }
        public PeerConfig Config { get; private set; }
        public PeerStatus Status { get; private set; }

        protected bool CanReceiveConnections { get; set; }

        private RawSocket socket;
        private List<Connection> connections;

        internal Peer(PeerConfig config)
        {
            Config = config;
            Status = PeerStatus.NotRunning;

            connections = new List<Connection>();
            socket = new RawSocket(config);
        }

        public override string ToString()
        {
            return $"[{ID}: {Status}]";
        }
    }
}