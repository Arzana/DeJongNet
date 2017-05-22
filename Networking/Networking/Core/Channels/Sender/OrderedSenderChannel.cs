namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System.Net;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class OrderedSenderChannel : SenderChannelBase
    {
        private int sequenceCount;

        public OrderedSenderChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        { }

        public override OutgoingMsg CreateMessage()
        {
            return new OutgoingMsg(ID, MsgType.Ordered, cache.Get());
        }

        public override OutgoingMsg CreateMessage(int initialSize)
        {
            return new OutgoingMsg(ID, MsgType.Ordered, cache.Get(initialSize));
        }

        public override void Heartbeat()
        {
            base.Heartbeat();
            sendPackets.Clear();
        }

        public override void EnqueueMessage(OutgoingMsg msg)
        {
            msg.SequenceNumber = GetClampSequenceCount();
            base.EnqueueMessage(msg);
        }

        private int GetClampSequenceCount()
        {
            return sequenceCount = ExtraMath.Clamp(++sequenceCount, 0, 1024);
        }
    }
}