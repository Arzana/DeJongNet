namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using Sender;

    internal sealed class ReliableOrderedReceiverChannel : OrderedReceiverChannel
    {
        private UnreliableSenderChannel ackSender;

        public ReliableOrderedReceiverChannel(RawSocket socket, Connection conn, UnreliableSenderChannel libSender, OrderChannelBehaviour behaviour)
            : base(socket, conn, behaviour)
        {
            ackSender = libSender;
        }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            ackSender.EnqueueMessage(MessageHelper.Ack(msg.Header.Type, ID, msg.Header.SequenceNumber));
            base.ReceiveMsg(msg);
        }
    }
}