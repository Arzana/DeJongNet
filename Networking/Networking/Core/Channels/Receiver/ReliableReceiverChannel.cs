namespace DeJong.Networking.Core.Channels.Receiver
{
    using Peers;
    using Messages;
    using Sender;
    using System.Net;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ReliableReceiverChannel : ReceiverChannelBase
    {
        private LibSenderChannel ackSender;

        public ReliableReceiverChannel(RawSocket socket, IPEndPoint remote, PeerConfig config, LibSenderChannel libSender)
            : base(socket, remote, config)
        {
            ackSender = libSender;
        }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            if (msg.Header.Type == MsgType.Reliable)
            {
                ackSender.EnqueueMessage(MessageHelper.Ack(ackSender.CreateMessage(MsgType.Acknowledge), msg.Header.Type, ID, msg.Header.SequenceNumber));
                base.ReceiveMsg(msg);
            }
            else Log.Warning(nameof(Peer), $"Received {msg.Header.Type} message on reliable receiver channel, message dropped");
        }
    }
}