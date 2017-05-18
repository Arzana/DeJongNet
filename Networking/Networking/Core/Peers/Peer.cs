using System.Collections.Generic;
using System.Net;

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
            socket.PacketReceived += ReceiveUnconnectedPacket;
        }

        public override string ToString()
        {
            return $"[{ID}: {Status}]";
        }

        public void Heartbeat()
        {
            socket.ReceivePacket();
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Heartbeat();
            }
        }

        private void ReceiveUnconnectedPacket(IPEndPoint sender, PacketReceiveEventArgs e)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].RemoteEndPoint.Equals(sender))
                {
                    connections[i].ReceivePacket(socket, e);
                }
            }

            HandleUnconnectedPacket(sender, e);
        }

        private void HandleUnconnectedPacket(IPEndPoint sender, PacketReceiveEventArgs e)
        {

        }
    }
}