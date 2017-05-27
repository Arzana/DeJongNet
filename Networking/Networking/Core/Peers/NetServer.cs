namespace DeJong.Networking.Core.Peers
{
    using Channels.Sender;
    using Messages;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

    /// <summary>
    /// Defines a peer that can receive incomming connections but can't initiate them.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetServer : Peer
    {
        /// <summary>
        /// Occurs when a discovery message was released.
        /// </summary>
        public event StrongEventHandler<IPEndPoint, EventArgs> OnDiscovery;
        /// <summary>
        /// Occurs when a <see cref="Connection"/> received a data message.
        /// </summary>
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;
        /// <summary>
        /// Occurs when a <see cref="Connection"/> is attempting to finalize its connection status.
        /// </summary>
        public event StrongEventHandler<Connection, SimpleMessageEventArgs> OnConnect;
        /// <summary>
        /// Occurs when a status of a <see cref="Connection"/> has changed.
        /// </summary>
        public event StrongEventHandler<Connection, StatusChangedEventArgs> OnStatusChanged;

        private ThreadSafeQueue<IPEndPoint> queuedDiscoveries;
        private ThreadSafeQueue<KeyValuePair<Connection, IncommingMsg>> queuedConnects;
        private ThreadSafeQueue<KeyValuePair<Connection, StatusChangedEventArgs>> queuedStatusChanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetServer"/> class with a specified configuration.
        /// </summary>
        /// <param name="config"> The way the <see cref="NetServer"/> should work. </param>
        public NetServer(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<IPEndPoint>();
            queuedConnects = new ThreadSafeQueue<KeyValuePair<Connection, IncommingMsg>>();
            queuedStatusChanges = new ThreadSafeQueue<KeyValuePair<Connection, StatusChangedEventArgs>>();
        }

        /// <summary>
        /// Approves a <see cref="Connection"/> wich status is <see cref="ConnectionStatus.RespondedAwaitingApproval"/>.
        /// </summary>
        /// <param name="connection"> The <see cref="Connection"/> to approve. </param>
        /// <param name="hail"> A hail message to send to the remote peer (Optional). </param>
        public void AcceptConnection(Connection connection, OutgoingMsg hail)
        {
            LoggedException.RaiseIf(connection.Status != ConnectionStatus.RespondedAwaitingApproval, nameof(Peer), $"Cannot accept connection while connection is {connection.Status}");
            connection.Status = ConnectionStatus.RespondedConnected;
            OutgoingMsg msg = MessageHelper.ConnectResponse(CreateMessage(MsgType.ConnectResponse, connection), Config.AppID, ID.ID, hail);
            connection.SendTo(msg);
        }

        /// <summary>
        /// Denies a <see cref="Connection"/> wich status is <see cref="ConnectionStatus.RespondedAwaitingApproval"/>.
        /// </summary>
        /// <param name="connection"> The <see cref="Connection"/> to deny. </param>
        /// <param name="reason"> The reason for the connection to be denied. </param>
        public void DenyConnection(Connection connection, string reason)
        {
            LoggedException.RaiseIf(connection.Status != ConnectionStatus.RespondedAwaitingApproval, nameof(Peer), $"Cannot deny connection while connection is {connection.Status}");
            connection.Status = ConnectionStatus.Disconnecting;
            OutgoingMsg msg = MessageHelper.Disconnect(CreateMessage(MsgType.Disconnect, connection), reason);
            connection.SendTo(msg);
            connection.Disconnected(reason);
        }

        /// <summary>
        /// Creates a new broadcast message on a specified channel.
        /// </summary>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(int channel)
        {
            OutgoingMsg result = CreateMessage(GetBroadcastConnection(), channel);
            result.IsBroadcast = true;
            return result;
        }

        /// <summary>
        /// Creates a new broadcast message on a specified channel.
        /// </summary>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <param name="initialSize"> The minimum size of the message. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(int channel, int initialSize)
        {
            OutgoingMsg result = CreateMessage(GetBroadcastConnection(), channel, initialSize);
            result.IsBroadcast = true;
            return result;
        }

        /// <summary>
        /// Creates a new message for a specified connection on a specified channel.
        /// </summary>
        /// <param name="conn"> The connection to create the message for. </param>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(Connection conn, int channel)
        {
            return conn.Sender[channel].CreateMessage();
        }

        /// <summary>
        /// Creates a new message for a specified connection on a specified channel.
        /// </summary>
        /// <param name="conn"> The connection to create the message for. </param>
        /// <param name="channel"> The indentifier for the channel. </param>
        /// <param name="initialSize"> The minimum size of the message. </param>
        /// <returns> A new message. </returns>
        public OutgoingMsg CreateMessage(Connection conn, int channel, int initialSize)
        {
            return conn.Sender[channel].CreateMessage(initialSize);
        }

        /// <summary>
        /// Sends a specified message to a specified connected peer.
        /// </summary>
        /// <param name="msg"> The message to send. </param>
        /// <param name="recipient"> The connected peer. </param>
        public void Send(OutgoingMsg msg, Connection recipient)
        {
            LoggedException.RaiseIf(recipient.Status != ConnectionStatus.Connected, nameof(Peer), $"Cannot send message to {recipient.Status} client");
            recipient.SendTo(msg);
        }

        /// <inheritdoc/>
        public override void PollMessages()
        {
            while (queuedDiscoveries.Count > 0)
            {
                EventInvoker.InvokeSafe(OnDiscovery, queuedDiscoveries.Dequeue(), EventArgs.Empty);
            }

            while (queuedConnects.Count > 0)
            {
                KeyValuePair<Connection, IncommingMsg> cur = queuedConnects.Dequeue();
                cur.Key.Status = ConnectionStatus.RespondedAwaitingApproval;

                if (Connections.Count < Config.MaximumConnections) EventInvoker.InvokeSafe(OnConnect, cur.Key, new SimpleMessageEventArgs(cur.Value));
                else DenyConnection(cur.Key, "Server is full");

                cur.Key.Sender.LibSender.Recycle(cur.Value);
            }

            while (queuedStatusChanges.Count > 0)
            {
                KeyValuePair<Connection, StatusChangedEventArgs> cur = queuedStatusChanges.Dequeue();
                EventInvoker.InvokeSafe(OnStatusChanged, cur.Key, cur.Value);
                cur.Key.Sender.LibSender.Recycle(cur.Value.Hail);
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
                    if (cur.Status == ConnectionStatus.Disconnected)
                    {
                        Connections.Remove(cur);
                        --i;
                    }
                }
            }
        }

        /// <summary>
        /// Sends a discovery response to a specified remote host.
        /// </summary>
        /// <param name="secMsg"> Data that specifies how the remote host should connect to the server (Optional). </param>
        /// <param name="remoteHost"> The remote host to send to. </param>
        public void SendDiscoveryResponse(OutgoingMsg secMsg, IPEndPoint remoteHost)
        {
            AddConnection(remoteHost, secMsg);
        }

        /// <inheritdoc/>
        protected override void Heartbeat()
        {
            base.Heartbeat();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connection cur = Connections[i];
                while (cur.Receiver[0].HasMessages) HandleLibMsgs(cur, cur.Receiver[0].DequeueMessage());
            }
        }

        /// <inheritdoc/>
        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg)
        {
            if (msg.LengthBits > LibHeader.SIZE_BITS) Log.Warning(nameof(Peer), $"Invalid discovery message received from remote host {sender}, message dropped");
            else queuedDiscoveries.Enqueue(sender);
        }

        /// <inheritdoc/>
        protected override void HandleDiscoveryResponse(IPEndPoint sender, IncommingMsg msg) { }

        private Connection GetBroadcastConnection()
        {
            LoggedException.RaiseIf(Connections.Count < 1, nameof(Peer), "Cannot create broadcast message when the server is empty");

            Connection genConn = null;
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Status == ConnectionStatus.Connected)
                {
                    genConn = Connections[i];
                    break;
                }
            }

            LoggedException.RaiseIf(genConn == null, nameof(Peer), "Cannot create broadcast message when no connection is connected");
            return genConn;
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
                case MsgType.Connect:
                    if (sender.Status != ConnectionStatus.ReceivedInitiation)
                    {
                        if (sender.Status != ConnectionStatus.RespondedAwaitingApproval) Log.Warning(nameof(Peer), $"Received unauthorized connect message from {sender.RemoteEndPoint}");
                        break;
                    }

                    string remoteAppID = msg.ReadString();
                    long remoteID = msg.ReadInt64();
                    sender.RemoteID = new NetID(remoteID);
                    if (remoteAppID != Config.AppID)
                    {
                        Log.Warning(nameof(Peer), $"{sender.RemoteEndPoint} attempted to conenct to unknown service {remoteAppID}");
                        DenyConnection(sender, "Invalid service ID");
                    }
                    else
                    {
                        queuedConnects.Enqueue(new KeyValuePair<Connection, IncommingMsg>(sender, msg));
                        sender.Status = ConnectionStatus.ReceivedInitiation;
                    }
                    break;
                case MsgType.ConnectionEstablished:
                    sender.Status = ConnectionStatus.Connected;
                    queuedStatusChanges.Enqueue(new KeyValuePair<Connection, StatusChangedEventArgs>(sender, new StatusChangedEventArgs((IncommingMsg)null)));
                    break;
                case MsgType.Disconnect:
                    string reason = msg.ReadString();
                    sender.Disconnected(reason);
                    queuedStatusChanges.Enqueue(new KeyValuePair<Connection, StatusChangedEventArgs>(sender, new StatusChangedEventArgs(reason)));
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
    }
}