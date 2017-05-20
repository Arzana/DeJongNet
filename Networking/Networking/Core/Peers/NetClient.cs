namespace DeJong.Networking.Core.Peers
{
    using Utilities.Logging;
    using Messages;
    using System.Net;
    using Utilities.Core;

    public sealed class NetClient : Peer
    {
        public event StrongEventHandler<Connection, DataMessageEventArgs> OnDataMessage;

        public NetClient(PeerConfig config)
            : base(config)
        { }

        public void DiscoverRemote(IPEndPoint host)
        {
            LoggedException.RaiseIf(Connections.Count > 0, nameof(Peer), "Cannot discovery new hosts whilst connected or connecting to other host");
            AddConnection(host);
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
                case MsgType.DiscoveryResponse:
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