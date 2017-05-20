namespace DeJong.Networking.Core.Peers
{
    using Utilities.Threading;
    using Messages;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Logging;
    using Channels;
    using Utilities.Core;

    public abstract class Peer : IFullyDisposable
    {
        public NetID ID { get; private set; }
        public PeerConfig Config { get; private set; }
        public PeerStatus Status { get; private set; }
        public List<Connection> Connections { get; private set; }
        public bool Disposed { get; private set; }
        public bool Disposing { get; private set; }

        private RawSocket socket;
        private ThreadSafeList<ChannelConfig> channels;
        private StopableThread networkThread;

        internal Peer(PeerConfig config)
        {
            Config = config;
            Status = PeerStatus.NotRunning;

            Connections = new List<Connection>();
            socket = new RawSocket(config);
            channels = new ThreadSafeList<ChannelConfig>();
            socket.PacketReceived += ReceiveUnconnectedPacket;
            networkThread = StopableThread.StartNew(Init, null, Heartbeat, Config.NetworkThreadName);
        }

        ~Peer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!(Disposed | Disposing))
            {
                Disposing = true;
                socket.Dispose();
                networkThread.StopWait();
                networkThread.Dispose();
                Disposed = true;
                Disposing = false;
            }
        }

        private void Init()
        {
            socket.Bind(false);
        }

        public override string ToString()
        {
            return $"[{ID}: {Status}]";
        }

        public void AddChannel(int id, DeliveryMethod type, OrderChannelBehaviour orderBehavior = OrderChannelBehaviour.None)
        {
            LoggedException.RaiseIf(type == DeliveryMethod.Unknown, nameof(Peer), $"Cannot add channel of type {type}");
            LoggedException.RaiseIf(channels.Contains(new ChannelConfig(id)), nameof(Peer), $"{type} channel with id: {id} already excists");
            channels.Add(new ChannelConfig(id, type, orderBehavior));

            for (int i = 0; i < Connections.Count; i++)
            {
                switch (type)
                {
                    case DeliveryMethod.Unreliable:
                        Connections[i].Receiver.AddUnreliable(id);
                        break;
                    case DeliveryMethod.Ordered:
                        Connections[i].Receiver.AddOrdered(id, orderBehavior);
                        break;
                    case DeliveryMethod.Reliable:
                        Connections[i].Receiver.AddReliable(id);
                        break;
                    case DeliveryMethod.ReliableOrdered:
                        Connections[i].Receiver.AddReliableOrdered(id, orderBehavior);
                        break;
                }
            }
        }

        public void Send(int channel, OutgoingMsg msg)
        {
            LoggedException.RaiseIf(channel == 0, nameof(Peer), "Cannot send messages over library channel");
            for (int i = 1; i < Connections.Count; i++)
            {
                Connections[i].SendTo(msg, channel);
            }
        }

        public abstract void PollMessages();
        protected abstract void HandleDiscovery(IPEndPoint sender, IncommingMsg msg);

        protected virtual void Heartbeat()
        {
            socket.ReceivePacket();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].Heartbeat();
            }
        }

        protected void AddConnection(IPEndPoint remote)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            conn.SendTo(MessageHelper.Discovery(), 0);
            Connections.Add(conn);
        }

        protected void AddConnection(IPEndPoint remote, OutgoingMsg secMsg)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            conn.SendTo(MessageHelper.DiscoveryResponse(secMsg), 0);
            Connections.Add(conn);
        }

        private void AddChannelsToConnection(Connection conn)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                ChannelConfig config = channels[i];

                switch (config.Type)
                {
                    case DeliveryMethod.Unreliable:
                        conn.Receiver.AddUnreliable(config.Id);
                        break;
                    case DeliveryMethod.Ordered:
                        conn.Receiver.AddOrdered(config.Id, config.Behavior);
                        break;
                    case DeliveryMethod.Reliable:
                        conn.Receiver.AddReliable(config.Id);
                        break;
                    case DeliveryMethod.ReliableOrdered:
                        conn.Receiver.AddReliableOrdered(config.Id, config.Behavior);
                        break;
                }
            }
        }

        private void ReceiveUnconnectedPacket(IPEndPoint sender, PacketReceiveEventArgs e)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].RemoteEndPoint.Equals(sender))
                {
                    Connections[i].ReceivePacket(socket, e);
                    return;
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
    }
}