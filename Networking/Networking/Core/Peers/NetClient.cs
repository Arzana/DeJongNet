namespace DeJong.Networking.Core.Peers
{
    using Messages;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetClient : Peer
    {
        public event StrongEventHandler<Connection, SimpleMessageEventArgs> OnDiscoveryResponse;
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;
        public event StrongEventHandler<Connection, StatusChangedEventArgs> OnStatusChanged;

        private ThreadSafeQueue<KeyValuePair<Connection, SimpleMessageEventArgs>> queuedDiscoveries;
        private IncommingMsg connectHail;
        private string disconnectReason;

        public NetClient(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<KeyValuePair<Connection, SimpleMessageEventArgs>>();
        }

        public void DiscoverLocal(int port)
        {
            DiscoverRemote(new IPEndPoint(NetUtils.GetBroadcastAddress(), port));
        }

        public void DiscoverRemote(string host, int port)
        {
            IPAddress address = NetUtils.ResolveAddress(host);
            if (address == null) return;

            DiscoverRemote(new IPEndPoint(address, port));
        }

        public void DiscoverRemote(IPEndPoint host)
        {
            LoggedException.RaiseIf(Connections.Count > 0, nameof(Peer), "Cannot discovery new hosts whilst connected or connecting to other host");
            Log.Verbose(nameof(Peer), $"Discovery send to {host}");
            AddConnection(host, true);
        }

        public void Disconnect(string reason)
        {
            OutgoingMsg msg = MessageHelper.Disconnect(reason);
            Connections[0].SendTo(msg, 0);
            Connections[0].Disconnect(reason);
        }

        public void Connect(Connection host, OutgoingMsg hail)
        {
            OutgoingMsg msg = MessageHelper.Connect(Config.AppID, ID.ID, hail);
            Connections[0].SendTo(msg, 0);
        }

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

        public override void PollMessages()
        {
            while (queuedDiscoveries.Count > 0)
            {
                KeyValuePair<Connection, SimpleMessageEventArgs> cur = queuedDiscoveries.Dequeue();
                EventInvoker.InvokeSafe(OnDiscoveryResponse, cur.Key, cur.Value);
            }

            if (connectHail != null)
            {
                EventInvoker.InvokeSafe(OnStatusChanged, Connections[0], new StatusChangedEventArgs(connectHail));
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
                    while (cur.Receiver[j].HasMessages) EventInvoker.InvokeSafe(OnDataMessage, cur, new DataMessageEventArgs(cur.Receiver[j].DequeueMessage()));
                }
            }
        }

        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg) { }

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
                    sender.SendTo(MessageHelper.Pong(msg.ReadInt32()), 0);
                    sender.SetPing(msg.ReadSingle());
                    break;
                case MsgType.Pong:
                    sender.ReceivePong(msg);
                    break;
                case MsgType.ConnectResponse:
                    string remoteAppID = msg.ReadString();
                    long remoteID = msg.ReadInt64();
                    if (remoteID != sender.RemoteID.ID) sender.RemoteID = new NetID(remoteID);
                    if (remoteAppID != Config.AppID) Log.Warning(nameof(Peer), $"Cannot conect to host application({remoteAppID}) as {Config.AppID}");
                    else
                    {
                        sender.Status = ConnectionStatus.Connected;
                        OutgoingMsg establish = MessageHelper.ConnectionEstablished();
                        sender.SendTo(establish, 0);
                        if (msg.PositionBits < msg.Header.PacketSize) connectHail = msg;
                        else connectHail = new IncommingMsg();
                    }
                    break;
                case MsgType.Acknowledge:
                    break;
                case MsgType.Disconnect:
                    sender.Disconnect(msg.ReadString());
                    break;
                default:
                    Log.Warning(nameof(Peer), $"{msg.Header.Type} message of size {msg.Header.PacketSize} send over library channel by {sender}, message dropped");
                    break;
            }
        }
    }
}