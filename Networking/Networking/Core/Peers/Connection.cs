namespace DeJong.Networking.Core.Peers
{
    using Utilities.Threading;
    using System.Net;
    using Messages;

    public sealed partial class Connection
    {
        public NetID ID { get; private set; }
        public ConnectionStatus Status { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        internal bool NeedsSending { get { return toSend.Count > 0; } }
        internal bool NeedsReceiving { get { return received.Count > 0; } }

        private ThreadSafeQueue<IncommingMsg> received;
        private ThreadSafeQueue<OutgoingMsg> toSend;
        private int pingCount;

        internal Connection(NetID id)
        {
            ID = id;
            received = new ThreadSafeQueue<IncommingMsg>();
            toSend = new ThreadSafeQueue<OutgoingMsg>();
        }

        internal IncommingMsg ReadMsg()
        {
            return received.Dequeue();
        }

        internal OutgoingMsg ReadBuffer()
        {
            return toSend.Dequeue();
        }
    }
}