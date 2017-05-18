namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Sender;
    using System.Net;

    internal sealed class ReliableReceiverChannel : ReceiverChannelBase
    {
        private UnreliableSenderChannel ackSender;

        public ReliableReceiverChannel(IPEndPoint remote, UnreliableSenderChannel libSender)
            : base(remote)
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