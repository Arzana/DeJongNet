namespace DeJong.Networking.Core.Channels.Sender
{
    using Utilities.Core;
    using Messages;
    using Peers;

    internal sealed class OrderedSenderChannel : SenderChannelBase
    {
        private int sequenceCount;

        public OrderedSenderChannel(RawSocket socket, Connection conn)
            : base(socket, conn.RemoteEndPoint)
        { }

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