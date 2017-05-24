namespace DeJong.Networking.Core.Peers
{
    using Channels;
    using Messages;
    using System;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

    /// <summary>
    /// Represent a base class for a networking client or server.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract class Peer : IFullyDisposable
    {
        /// <summary>
        /// Gets the indentifier of the <see cref="Peer"/>.
        /// </summary>
        public NetID ID { get; private set; }
        /// <summary>
        /// Gets the <see cref="PeerConfig"/> used to initialize this <see cref="Peer"/>.
        /// </summary>
        public PeerConfig Config { get; private set; }
        /// <summary>
        /// Gets the current status of this <see cref="Peer"/>.
        /// </summary>
        public PeerStatus Status { get; private set; }
        /// <summary>
        /// Gets the connections of this <see cref="Peer"/>.
        /// </summary>
        public ThreadSafeList<Connection> Connections { get; private set; }
        /// <inheritdoc/>
        public bool Disposed { get; private set; }
        /// <inheritdoc/>
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

        /// <summary>
        /// Disposes and finalizes the <see cref="Peer"/>.
        /// </summary>
        ~Peer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Shut the server down for a specified reason.
        /// </summary>
        /// <param name="reason"> The reason for this shutdown. </param>
        public void ShutDown(string reason)
        {
            Status = PeerStatus.ShutdownRequested;
            OutgoingMsg msg = MessageHelper.Disconnect(CreateMessage(MsgType.Disconnect), reason);
            networkThread.StopWait();

            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].SendTo(msg);
            }

            Heartbeat();

            Connections.Clear();
            socket.UnBind();
            Status = PeerStatus.NotRunning;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Config.AppID} ({ID}): {Status}]";
        }

        /// <summary>
        /// Adds a specified channel to this <see cref="Peer"/> (max 15 channels).
        /// </summary>
        /// <param name="id"> The unique indentifier for the channel. </param>
        /// <param name="type"> The type of channel to create. </param>
        /// <param name="orderBehavior"> The behaviour of the channel if it is <see cref="DeliveryMethod.Ordered"/> or <see cref="DeliveryMethod.ReliableOrdered"/>. </param>
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

        /// <summary>
        /// Broadcasts a specified message to all connected peers.
        /// </summary>
        /// <param name="msg"> The message to broadcast. </param>
        public void Send(OutgoingMsg msg)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Status == ConnectionStatus.Connected) Connections[i].SendTo(msg);
            }
        }

        /// <summary>
        /// Polls all incomming messages and events for this <see cref="Peer"/>.
        /// </summary>
        public abstract void PollMessages();

        internal OutgoingMsg CreateMessage(MsgType type, Connection conn = null)
        {
            conn = conn ?? Connections[0];
            return conn.Sender.LibSender.CreateMessage(type);
        }

        /// <summary>
        /// Disposes the <see cref="Peer"/>.
        /// </summary>
        /// <param name="disposing"> Not used. </param>
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

        /// <summary>
        /// Defines how an incomming discovery message should be handled.
        /// </summary>
        /// <param name="sender"> The remote host that send the discovery request. </param>
        /// <param name="msg"> The message data. </param>
        protected abstract void HandleDiscovery(IPEndPoint sender, IncommingMsg msg);
        /// <summary>
        /// Defines how an incomming discovery response message should be handled.
        /// </summary>
        /// <param name="sender"> The remote host that send the discovery request. </param>
        /// <param name="msg"> The message data. </param>
        protected abstract void HandleDiscoveryResponse(IPEndPoint sender, IncommingMsg msg);

        /// <summary>
        /// Updates the <see cref="Peer"/> and its underlying components.
        /// </summary>
        protected virtual void Heartbeat()
        {
            socket.ReceivePacket();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].Heartbeat();
            }
        }

        /// <summary>
        /// Adds a <see cref="Connection"/> to the <see cref="Peer"/>.
        /// </summary>
        /// <param name="remote"> The remote host to create a connection for. </param>
        /// <param name="sendDiscovery"> Whether a discovery message should be send to the remote host. </param>
        protected void AddConnection(IPEndPoint remote, bool sendDiscovery)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            Connections.Add(conn);
            if (sendDiscovery) conn.SendTo(CreateMessage(MsgType.Discovery));
        }

        /// <summary>
        /// Adds a <see cref="Connection"/> to the <see cref="Peer"/>.
        /// </summary>
        /// <param name="remote"> The remote host to create a connection for. </param>
        /// <param name="secMsg"> The security message to send to the remote host. </param>
        protected void AddConnection(IPEndPoint remote, OutgoingMsg secMsg)
        {
            Connection conn = new Connection(socket, remote, Config);
            AddChannelsToConnection(conn);
            Connections.Add(conn);
            conn.SendTo(MessageHelper.DiscoveryResponse(CreateMessage(MsgType.DiscoveryResponse), secMsg));
        }

        private void Init()
        {
            Status = PeerStatus.Starting;
            socket.Bind(false);
            ID = new NetID(NetUtils.GetID(socket.BoundEP));
            Status = PeerStatus.Running;
            Log.Info(nameof(Peer), $"Peer {ID} starting");
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