namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using Sender;
    using System.Net;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ReliableOrderedReceiverChannel : OrderedReceiverChannel
    {
        private LibSenderChannel ackSender;

        public ReliableOrderedReceiverChannel(RawSocket socket, IPEndPoint remote, PeerConfig config, LibSenderChannel libSender, OrderChannelBehaviour behaviour)
            : base(socket, remote, config, behaviour)
        {
            ackSender = libSender;
        }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            if (msg.Header.Type == MsgType.ReliableOrdered)
            {
                ackSender.EnqueueMessage(MessageHelper.Ack(ackSender.CreateMessage(MsgType.Acknowledge), msg.Header.Type, ID, msg.Header.SequenceNumber));
                base.ReceiveMsg(msg);
            }
            else Log.Warning(nameof(Peer), $"Received {msg.Header.Type} message on reliableOrdered receiver channel, message dropped");
        }
    }
}