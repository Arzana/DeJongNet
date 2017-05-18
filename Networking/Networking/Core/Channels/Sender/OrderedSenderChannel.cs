namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System.Net;
    using Utilities.Core;

    internal sealed class OrderedSenderChannel : SenderChannelBase
    {
        private int sequenceCount;

        public OrderedSenderChannel(RawSocket socket, IPEndPoint remote)
            : base(socket, remote)
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