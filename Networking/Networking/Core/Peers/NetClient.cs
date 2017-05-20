namespace DeJong.Networking.Core.Peers
{
    using Utilities.Logging;
    using Messages;
    using System.Net;
    using Utilities.Core;
    using Utilities.Threading;
    using System.Collections.Generic;
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetClient : Peer
    {
        public event StrongEventHandler<Connection, DiscoveryResponseEventArgs> OnDiscoveryResponse;
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;

        private ThreadSafeQueue<KeyValuePair<Connection, DiscoveryResponseEventArgs>> queuedDiscoveries;

        public NetClient(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<KeyValuePair<Connection, DiscoveryResponseEventArgs>>();
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
                KeyValuePair<Connection, DiscoveryResponseEventArgs> cur = queuedDiscoveries.Dequeue();
                EventInvoker.InvokeSafe(OnDiscoveryResponse, cur.Key, cur.Value);
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
            queuedDiscoveries.Enqueue(new KeyValuePair<Connection, DiscoveryResponseEventArgs>(Connections[0], new DiscoveryResponseEventArgs(msg)));
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
                case MsgType.Connect:
                    break;
                case MsgType.ConnectResponse:
                    break;
                case MsgType.ConnectionEstablished:
                    break;
                case MsgType.Acknowledge:
                    break;
                case MsgType.Disconnect:
                    sender.Disconnect(msg.ReadString());
                    Connections.Remove(sender);
                    break;
                case MsgType.DiscoveryResponse:
                    queuedDiscoveries.Enqueue(new KeyValuePair<Connection, DiscoveryResponseEventArgs>(sender, new DiscoveryResponseEventArgs(msg)));
                    break;
                case MsgType.LibraryError:
                case MsgType.Unreliable:
                case MsgType.Ordered:
                case MsgType.Reliable:
                case MsgType.ReliableOrdered:
                case MsgType.Discovery:
                default:
                    Log.Warning(nameof(Peer), $"{msg.Header.Type} message of size {msg.Header.PacketSize} send over library channel by {sender}, message dropped");
                    break;
            }
        }
    }
}