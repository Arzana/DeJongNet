namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using System;
    using Utilities.Threading;

    internal class OrderedReceiverChannel : ReceiverChannelBase
    {
        public override bool HasMessages { get { return released.Count > 0; } }

        private OrderChannelBehaviour behaviour;
        private int sequenceCount;
        private ThreadSafeQueue<IncommingMsg> released;

        public OrderedReceiverChannel(RawSocket socket, Connection conn, OrderChannelBehaviour behaviour)
            : base(socket, conn.RemoteEndPoint)
        {
            this.behaviour = behaviour;
            released = new ThreadSafeQueue<IncommingMsg>();
        }

        public override void Heartbeat()
        {
            if (received.Count > 1)
            {
                IncommingMsg[] holder = new IncommingMsg[received.Count];
                received.CopyTo(holder, 0);
                Array.Sort(holder, (f, s) =>
                {
                    if (f.Header.SequenceNumber < s.Header.SequenceNumber) return -1;
                    if (f.Header.SequenceNumber > s.Header.SequenceNumber) return 1;
                    return 0;
                });

                if (behaviour == OrderChannelBehaviour.Order) released.Enqueue(holder);
                else released.Enqueue(holder[holder.Length - 1]);
            }

            base.Heartbeat();
        }

        public override IncommingMsg DequeueMessage()
        {
            return released.Dequeue();
        }
    }
}