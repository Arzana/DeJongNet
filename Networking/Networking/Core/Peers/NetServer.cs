namespace DeJong.Networking.Core.Peers
{
    using Messages;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetServer : Peer
    {
        public event StrongEventHandler<IPEndPoint, EventArgs> OnDiscovery;
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;
        public event StrongEventHandler<Connection, SimpleMessageEventArgs> OnConnect;
        public event StrongEventHandler<Connection, StatusChangedEventArgs> OnStatusChanged;

        private ThreadSafeQueue<IPEndPoint> queuedDiscoveries;
        private ThreadSafeQueue<KeyValuePair<Connection, IncommingMsg>> queuedConnects;
        private ThreadSafeQueue<KeyValuePair<Connection, StatusChangedEventArgs>> queuedStatusChanges;

        public NetServer(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<IPEndPoint>();
            queuedConnects = new ThreadSafeQueue<KeyValuePair<Connection, IncommingMsg>>();
            queuedStatusChanges = new ThreadSafeQueue<KeyValuePair<Connection, StatusChangedEventArgs>>();
        }

        public void Send(Connection to, int channel, OutgoingMsg msg)
        {
            LoggedException.RaiseIf(channel == 0, nameof(Peer), "Cannot send messages over library channel");
            LoggedException.RaiseIf(to.Status != ConnectionStatus.Connected, nameof(Peer), "Cannot send messages to unconnected client");
            to.SendTo(msg, channel);
        }

        public void AcceptConnection(Connection connection, OutgoingMsg hail)
        {
            LoggedException.RaiseIf(connection.Status != ConnectionStatus.RespondedAwaitingApproval, nameof(Peer), $"Cannot accept connection while connection is {connection.Status}");
            connection.Status = ConnectionStatus.RespondedConnected;
            OutgoingMsg msg = MessageHelper.ConnectResponse(Config.AppID, ID.ID, hail);
            connection.SendTo(msg, 0);
        }

        public void DenyConnection(Connection connection, string reason)
        {
            LoggedException.RaiseIf(connection.Status != ConnectionStatus.RespondedAwaitingApproval, nameof(Peer), $"Cannot deny connection while connection is {connection.Status}");
            connection.Status = ConnectionStatus.Disconnecting;
            OutgoingMsg msg = MessageHelper.Disconnect(reason);
            connection.SendTo(msg, 0);
            connection.Disconnect(reason);
        }

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
                EventInvoker.InvokeSafe(OnConnect, cur.Key, new SimpleMessageEventArgs(cur.Value));
            }

            while (queuedStatusChanges.Count > 0)
            {
                KeyValuePair<Connection, StatusChangedEventArgs> cur = queuedStatusChanges.Dequeue();
                EventInvoker.InvokeSafe(OnStatusChanged, cur.Key, cur.Value);
            }

            for (int i = 0; i < Connections.Count; i++)
            {
                Connection cur = Connections[i];
                for (int j = 1; j < cur.Receiver.Size; j++)
                {
                    while (cur.Receiver[j].HasMessages) EventInvoker.InvokeSafe(OnDataMessage, cur, new DataMessageEventArgs(cur.Receiver[j].DequeueMessage()));
                    if (cur.Status == ConnectionStatus.Disconnected)
                    {
                        Connections.Remove(cur);
                        --i;
                    }
                }
            }
        }

        public void SendDiscoveryResponse(OutgoingMsg secMsg, IPEndPoint remoteHost)
        {
            AddConnection(remoteHost, secMsg);
        }

        protected override void Heartbeat()
        {
            base.Heartbeat();
            for (int i = 0; i < Connections.Count; i++)
            {
                Connection cur = Connections[i];
                while (cur.Receiver[0].HasMessages) HandleLibMsgs(cur, cur.Receiver[0].DequeueMessage());
            }
        }

        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg)
        {
            if (msg.LengthBits > LibHeader.SIZE_BITS) Log.Warning(nameof(Peer), $"Invalid discovery message received from remote host {sender}, message dropped");
            else queuedDiscoveries.Enqueue(sender);
        }

        protected override void HandleDiscoveryResponse(IPEndPoint sender, IncommingMsg msg) { }

        private void HandleLibMsgs(Connection sender, IncommingMsg msg)
        {
            switch (msg.Header.Type)
            {
                case MsgType.Ping:
                    sender.SendTo(MessageHelper.Pong(msg.ReadInt32()), 0);
                    sender.SetPing(msg.ReadSingle());
                    break;
                case MsgType.Pong:
                    sender.ReceivePong(msg);
                    break;
                case MsgType.Connect:
                    int oldPos = msg.PositionBits;
                    msg.ReadString();
                    long remoteID = msg.ReadInt64();
                    msg.PositionBits = oldPos;

                    if (sender.RemoteID != remoteID) sender.RemoteID = new NetID(remoteID);

                    queuedConnects.Enqueue(new KeyValuePair<Connection, IncommingMsg>(sender, msg));
                    sender.Status = ConnectionStatus.ReceivedInitiation;
                    break;
                case MsgType.ConnectionEstablished:
                    sender.Status = ConnectionStatus.Connected;
                    queuedStatusChanges.Enqueue(new KeyValuePair<Connection, StatusChangedEventArgs>(sender, new StatusChangedEventArgs((IncommingMsg)null)));
                    break;
                case MsgType.Acknowledge:
                    break;
                case MsgType.Disconnect:
                    string reason = msg.ReadString();
                    sender.Disconnect(reason);
                    queuedStatusChanges.Enqueue(new KeyValuePair<Connection, StatusChangedEventArgs>(sender, new StatusChangedEventArgs(reason)));
                    break;
                default:
                    Log.Warning(nameof(Peer), $"{msg.Header.Type} message of size {msg.Header.PacketSize} send over library channel by {sender}, message dropped");
                    break;
            }
        }
    }
}