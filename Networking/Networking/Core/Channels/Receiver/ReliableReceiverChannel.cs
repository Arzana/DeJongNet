namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using Sender;

    internal sealed class ReliableReceiverChannel : ReceiverChannelBase
    {
        private UnreliableSenderChannel ackSender;

        public ReliableReceiverChannel(RawSocket socket, Connection conn, UnreliableSenderChannel libSender)
            : base(socket, conn.RemoteEndPoint)
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