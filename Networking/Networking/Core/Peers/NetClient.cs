namespace DeJong.Networking.Core.Peers
{
    using Channels.Sender;
    using Messages;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

    /// <summary>
    /// Defines a peer that can't receive incomming connections but can initiate them.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetClient : Peer
    {
        /// <summary>
        /// Occurs when a remote host responded to our discovery request.
        /// </summary>
        public event StrongEventHandler<Connection, SimpleMessageEventArgs> OnDiscoveryResponse;
        /// <summary>
        /// Occurs when a <see cref="Connection"/> received a data message.
        /// </summary>
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;
        /// <summary>
        /// Occurs when a status of a <see cref="Connection"/> has changed.
        /// </summary>
        public event StrongEventHandler<Connection, StatusChangedEventArgs> OnStatusChanged;

        private ThreadSafeQueue<KeyValuePair<Connection, SimpleMessageEventArgs>> queuedDiscoveries;
        private IncommingMsg connectHail;
        private string disconnectReason;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetClient"/> class with a specified configuration.
        /// </summary>
        /// <param name="config"> The way the <see cref="NetServer"/> should work. </param>
        public NetClient(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<KeyValuePair<Connection, SimpleMessageEventArgs>>();
        }

        /// <summary>
        /// Broadcasts a discovery message to the local network.
        /// </summary>
        /// <param name="port"> The port to discover services on. </param>
        public void DiscoverLocal(int port)
        {
            DiscoverRemote(new IPEndPoint(NetUtils.GetBroadcastAddress(), port));
        }

        /// <summary>
        /// Sends a discovery message to the specified remote host.
        /// </summary>
        /// <param name="host"> A representation of the remote host. </param>
        /// <param name="port"> The port to discover services on. </param>
        public void DiscoverRemote(string host, int port)
        {
            IPAddress address = NetUtils.ResolveAddress(host);
            if (address == null) return;

            DiscoverRemote(new IPEndPoint(address, port));
        }

        /// <summary>
        /// Sends a discovery message to the specified remote host.
        /// </summary>
        /// <param name="host"> The ip address and port to discover services on. </param>
        public void DiscoverRemote(IPEndPoint host)
        {
            LoggedException.RaiseIf(Connections.Count > 0, nameof(Peer), "Cannot discovery new hosts whilst connected or connecting to other host");
            Log.Verbose(nameof(Peer), $"Discovery send to {host}");
            AddConnection(host, true);
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        /// <param name="reason"> The reason for disconneting. </param>
        public void Disconnect(string reason)
        {
            CheckConnection();
            Connections[0].Disconnect(reason);
        }

        /// <summary>
        /// Connects to the specified server.
        /// </summary>
        /// <param name="host"> The remote host to connect to. </param>
        /// <param name="hail"> The hail or security message to send (Optional). </param>
        public void Connect(Connection host, OutgoingMsg hail)
        {
            OutgoingMsg msg = MessageHelper.Connect(CreateMessage(MsgType.Connect), Config.AppID, ID.ID, hail);
            Connections[0].SendTo(msg);
        }

        /// <summary>
        /// Creates a new message on a specified channel.
        /// </summary>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(int channel)
        {
            CheckConnection();
            return Connections[0].Sender[channel].CreateMessage();
        }

        /// <summary>
        /// Creates a new message on a specified channel.
        /// </summary>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <param name="initialSize"> The minimum size of the message. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(int channel, int initialSize)
        {
            CheckConnection();
            return Connections[0].Sender[channel].CreateMessage(initialSize);
        }

        /// <inheritdoc/>
        public override void PollMessages()
        {
            while (queuedDiscoveries.Count > 0)
            {
                KeyValuePair<Connection, SimpleMessageEventArgs> cur = queuedDiscoveries.Dequeue();
                EventInvoker.InvokeSafe(OnDiscoveryResponse, cur.Key, cur.Value);
                cur.Key.Receiver[0].Recycle(cur.Value.Message);
            }

            if (connectHail != null)
            {
                EventInvoker.InvokeSafe(OnStatusChanged, Connections[0], new StatusChangedEventArgs(connectHail));
                Connections[0].Receiver[0].Recycle(connectHail);
                connectHail = null;
            }
            if (disconnectReason != null)
            {
                EventInvoker.InvokeSafe(OnStatusChanged, Connections[0], new StatusChangedEventArgs(disconnectReason));
                disconnectReason = null;
            }

            for (int i = 0; i < Connections.Count; i++)
            {
                Connection cur = Connections[i];
                for (int j = 1; j < cur.Receiver.Size; j++)
                {
                    while (cur.Receiver[j].HasMessages)
                    {
                        DataMessageEventArgs args = new DataMessageEventArgs(cur.Receiver[j].DequeueMessage());
                        EventInvoker.InvokeSafe(OnDataMessage, cur, args);
                        cur.Receiver[j].Recycle(args.Message);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void Heartbeat()
        {
            base.Heartbeat();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connection cur = Connections[i];
                while (cur.Receiver[0].HasMessages) HandleLibMsgs(cur, cur.Receiver[0].DequeueMessage());
                if (cur.Status == ConnectionStatus.Disconnected)
                {
                    Connections.Remove(cur);
                    --i;
                }
            }
        }

        /// <inheritdoc/>
        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg) { }
        /// <inheritdoc/>
        protected override void HandleDiscoveryResponse(IPEndPoint sender, IncommingMsg msg)
        {
            Connections.Clear();
            AddConnection(sender, false);
            queuedDiscoveries.Enqueue(new KeyValuePair<Connection, SimpleMessageEventArgs>(Connections[0], new SimpleMessageEventArgs(msg)));
        }

        private void HandleLibMsgs(Connection sender, IncommingMsg msg)
        {
            switch (msg.Header.Type)
            {
                case MsgType.Ping:
                    sender.SendTo(MessageHelper.Pong(CreateMessage(MsgType.Pong, sender), msg.ReadInt32()));
                    sender.SetPing(msg.ReadSingle());
                    break;
                case MsgType.Pong:
                    sender.ReceivePong(msg);
                    break;
                case MsgType.ConnectResponse:
                    string remoteAppID = msg.ReadString();
                    sender.RemoteID = new NetID(msg.ReadInt64());
                    if (remoteAppID != Config.AppID) Log.Warning(nameof(Peer), $"Cannot conect to host application({remoteAppID}) as {Config.AppID}");
                    else
                    {
                        sender.Status = ConnectionStatus.Connected;
                        OutgoingMsg establish = MessageHelper.ConnectionEstablished(CreateMessage(MsgType.ConnectionEstablished, sender));
                        sender.SendTo(establish);
                        if (msg.PositionBits < msg.Header.PacketSize) connectHail = msg;
                        else connectHail = sender.Receiver[0].CreateMessage(LibHeader.Empty);
                    }
                    break;
                case MsgType.Disconnect:
                    sender.Disconnected(msg.ReadString());
                    break;
                case MsgType.Acknowledge:
                    msg.SkipPadBits(4);
                    int channel = msg.ReadPadBits(4);
                    ((ReliableSenderChannel)sender.Sender[channel]).ReceiveAck(msg.ReadInt16());
                    break;
                default:
                    Log.Warning(nameof(Peer), $"{msg.Header.Type} message of size {msg.Header.PacketSize} send over library channel by {sender}, message dropped");
                    break;
            }
        }

        private void CheckConnection()
        {
            LoggedException.RaiseIf(Connections.Count < 1, nameof(Peer), "Server must be connected in order to send messages");
            LoggedException.RaiseIf(Connections[0].Status != ConnectionStatus.Connected, nameof(Peer), "Connecting needs to be connected before messsages can be send");
        }
    }
}