namespace DeJong.Networking.Core.Channels.Receiver
{
    using Utilities.Logging;
    using Messages;
    using System;
    using System.Net;
    using Utilities.Threading;
    using Peers;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal class OrderedReceiverChannel : ReceiverChannelBase
    {
        public override bool HasMessages { get { return released.Count > 0; } }

        private OrderChannelBehaviour behaviour;
        private int sequenceCount;
        private ThreadSafeQueue<IncommingMsg> released;

        public OrderedReceiverChannel(RawSocket socket, IPEndPoint remote, PeerConfig config, OrderChannelBehaviour behaviour)
            : base(socket, remote, config)
        {
            this.behaviour = behaviour;
            released = new ThreadSafeQueue<IncommingMsg>();
        }

        public override void Heartbeat()
        {
            if (received.Count > 0)
            {
                IncommingMsg[] holder = new IncommingMsg[received.Count];
                received.CopyTo(holder, 0);
                received.Clear();

                if (holder.Length > 1)
                {
                    Array.Sort(holder, (f, s) =>
                    {
                        if (f.Header.SequenceNumber < s.Header.SequenceNumber) return -1;
                        if (f.Header.SequenceNumber > s.Header.SequenceNumber) return 1;
                        return 0;
                    });
                }

                if (behaviour == OrderChannelBehaviour.Order) released.Enqueue(holder);
                else released.Enqueue(holder[holder.Length - 1]);
            }

            base.Heartbeat();
        }

        public override IncommingMsg DequeueMessage()
        {
            return released.Dequeue();
        }

        protected override void Dispose(bool disposing)
        {
            if (!(Disposed | Disposing)) released.Dispose();
            base.Dispose(disposing);
        }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            if (msg.Header.Type == MsgType.Ordered)
            {
                if (msg.Header.SequenceNumber > sequenceCount)
                {
                    sequenceCount = msg.Header.SequenceNumber;
                    base.ReceiveMsg(msg);
                }
                else if (msg.Header.SequenceNumber == 0 && sequenceCount == short.MaxValue)
                {
                    sequenceCount = 0;
                    base.ReceiveMsg(msg);
                }
            }
            else Log.Warning(nameof(Peer), $"Received {msg.Header.Type} message on ordered receiver channel, message dropped");
        }
    }
}