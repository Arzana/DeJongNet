namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Sender;
    using System.Net;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ReliableOrderedReceiverChannel : OrderedReceiverChannel
    {
        private UnreliableSenderChannel ackSender;

        public ReliableOrderedReceiverChannel(IPEndPoint remote, UnreliableSenderChannel libSender, OrderChannelBehaviour behaviour)
            : base(remote, behaviour)
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