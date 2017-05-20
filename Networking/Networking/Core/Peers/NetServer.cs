namespace DeJong.Networking.Core.Peers
{
    using Messages;
    using System;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;
    using Utilities.Threading;

    public sealed class NetServer : Peer
    {
        public event StrongEventHandler<IPEndPoint, EventArgs> OnDiscovery;
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;

        private ThreadSafeQueue<IPEndPoint> queuedDiscoveries;

        public NetServer(PeerConfig config)
            : base(config)
        {
            queuedDiscoveries = new ThreadSafeQueue<IPEndPoint>();
        }

        public void Send(Connection to, int channel, OutgoingMsg msg)
        {
            LoggedException.RaiseIf(channel == 0, nameof(Peer), "Cannot send messages over library channel");
            LoggedException.RaiseIf(to.Status != ConnectionStatus.Connected, nameof(Peer), "Cannot send messages to unconnected client");
            to.SendTo(msg, channel);
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

        public override void PollMessages()
        {
            while (queuedDiscoveries.Count > 0)
            {
                EventInvoker.InvokeSafe(OnDiscovery, queuedDiscoveries.Dequeue(), EventArgs.Empty);
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

        public void SendDiscoveryResponse(OutgoingMsg secMsg, IPEndPoint remoteHost)
        {
            AddConnection(remoteHost, secMsg);
        }

        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg)
        {
            if (msg.LengthBits > LibHeader.SIZE_BITS) Log.Warning(nameof(Peer), $"Invalid discovery message received from remote host {sender}, message dropped");
            else queuedDiscoveries.Enqueue(sender);
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
                    break;
                case MsgType.LibraryError:
                case MsgType.Unreliable:
                case MsgType.Ordered:
                case MsgType.Reliable:
                case MsgType.ReliableOrdered:
                case MsgType.Discovery:
                case MsgType.DiscoveryResponse:
                default:
                    Log.Warning(nameof(Peer), $"{msg.Header.Type} message of size {msg.Header.PacketSize} send over library channel by {sender}, message dropped");
                    break;
            }
        }
    }
}