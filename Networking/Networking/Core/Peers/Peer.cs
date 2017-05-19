using DeJong.Networking.Core.Messages;
using DeJong.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace DeJong.Networking.Core.Peers
{
    public abstract class Peer
    {
        public NetID ID { get; private set; }
        public PeerConfig Config { get; private set; }
        public PeerStatus Status { get; private set; }

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

        public void Init()
        {
            socket.Bind(false);
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

        protected void AddConnection(IPEndPoint remote)
        {
            Connection conn = new Connection(socket, remote, Config);
            conn.SendTo(MessageHelper.Discovery(), 0);
            connections.Add(conn);
        }

        protected void AddConnection(IPEndPoint remote, OutgoingMsg secMsg)
        {
            Connection conn = new Connection(socket, remote, Config);
            conn.SendTo(MessageHelper.DiscoveryResponse(secMsg), 0);
            connections.Add(conn);
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
            byte[] data = new byte[e.PacketSize];
            Array.Copy(socket.ReceiveBuffer, 0, data, 0, e.PacketSize);
            IncommingMsg msg = new IncommingMsg(data);

            if (msg.Header.Channel == 0 && msg.Header.Type == MsgType.Discovery) HandleDiscovery(sender, msg);
            else Log.Warning(nameof(Peer), $"Unconnected data received from remote host {sender}, message dropped");
        }

        protected abstract void HandleDiscovery(IPEndPoint sender, IncommingMsg msg);
    }
}