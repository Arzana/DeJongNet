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

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract class Peer : IFullyDisposable
    {
        public NetID ID { get; private set; }
        public PeerConfig Config { get; private set; }
        public PeerStatus Status { get; private set; }
        public ThreadSafeList<Connection> Connections { get; private set; }
        public bool Disposed { get; private set; }
        public bool Disposing { get; private set; }

        private RawSocket socket;
        private ThreadSafeList<ChannelConfig> channels;
        private StopableThread networkThread;

        internal Peer(PeerConfig config)
        {
            Config = config;
            Status = PeerStatus.NotRunning;

            Connections = new ThreadSafeList<Connection>();
            socket = new RawSocket(config);
            channels = new ThreadSafeList<ChannelConfig>();
            socket.PacketReceived += ReceiveUnconnectedPacket;
            networkThread = StopableThread.StartNew(Init, null, Heartbeat, Config.NetworkThreadName);
        }

        ~Peer()
        {
            Dispose(false);
        }

        public void ShutDown(string reason)
        {
            OutgoingMsg msg = MessageHelper.Disconnect(CreateMessage(MsgType.Disconnect), reason);
            networkThread.StopWait();

            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].SendTo(msg);
            }

            Heartbeat();

            Connections.Clear();
            socket.UnBind();
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
                ShutDown("Wrongfully disposed");
                socket.Dispose();
                networkThread.StopWait();
                networkThread.Dispose();
                Disposed = true;
                Disposing = false;
            }
        }

        private void Init()
        {
            Status = PeerStatus.Starting;
            socket.Bind(false);
            ID = new NetID(NetUtils.GetID(socket.BoundEP));
            Status = PeerStatus.Running;
            Log.Info(nameof(Peer), $"Peer {ID} starting");
        }

        public override string ToString()
        {
            return $"[{Config.AppID} ({ID}): {Status}]";
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
                        Connections[i].Sender.AddUnreliable(id);
                        break;
                    case DeliveryMethod.Ordered:
                        Connections[i].Receiver.AddOrdered(id, orderBehavior);
                        Connections[i].Sender.AddOrdered(id);
                        break;
                    case DeliveryMethod.Reliable:
                        Connections[i].Sender.AddReliable(id);
                        Connections[i].Receiver.AddReliable(id);
                        break;
                    case DeliveryMethod.ReliableOrdered:
                        Connections[i].Receiver.AddReliableOrdered(id, orderBehavior);
                        Connections[i].Sender.AddReliableOrdered(id);
                        break;
                }
            }
        }

        public void Send(OutgoingMsg msg)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Status == ConnectionStatus.Connected) Connections[i].SendTo(msg);
            }
        }

        internal OutgoingMsg CreateMessage(MsgType type, Connection conn = null)
        {
            conn = conn ?? Connections[0];
            return conn.Sender.LibSender.CreateMessage(type);
        }

        public abstract void PollMessages();
        protected abstract void HandleDiscovery(IPEndPoint sender, IncommingMsg msg);
        protected abstract void HandleDiscoveryResponse(IPEndPoint sender, IncommingMsg msg);

        protected virtual void Heartbeat()
        {
            socket.ReceivePacket();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].Heartbeat();
            }
        }

        protected void AddConnection(IPEndPoint remote, bool sendDiscovery)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            Connections.Add(conn);
            if (sendDiscovery) conn.SendTo(CreateMessage(MsgType.Discovery));
        }

        protected void AddConnection(IPEndPoint remote, OutgoingMsg secMsg)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            Connections.Add(conn);
            conn.SendTo(MessageHelper.DiscoveryResponse(CreateMessage(MsgType.DiscoveryResponse), secMsg));
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
                        conn.Sender.AddUnreliable(config.Id);
                        break;
                    case DeliveryMethod.Ordered:
                        conn.Receiver.AddOrdered(config.Id, config.Behavior);
                        conn.Sender.AddOrdered(config.Id);
                        break;
                    case DeliveryMethod.Reliable:
                        conn.Receiver.AddReliable(config.Id);
                        conn.Sender.AddReliable(config.Id);
                        break;
                    case DeliveryMethod.ReliableOrdered:
                        conn.Receiver.AddReliableOrdered(config.Id, config.Behavior);
                        conn.Sender.AddReliableOrdered(config.Id);
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
            msg.Header = new LibHeader(msg);

            if (msg.Header.Channel == 0 && msg.Header.Type == MsgType.Discovery) HandleDiscovery(sender, msg);
            else if (msg.Header.Channel == 0 && msg.Header.Type == MsgType.DiscoveryResponse) HandleDiscoveryResponse(sender, msg);
            else Log.Warning(nameof(Peer), $"Unconnected data received from remote host {sender}, message dropped");
        }
    }
}