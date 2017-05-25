namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using System.Net;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class LibReceiverChannel : ReceiverChannelBase
    {
        public LibReceiverChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        { }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            if (msg.Header.Type != MsgType.Unreliable &&
                msg.Header.Type != MsgType.Reliable &&
                msg.Header.Type != MsgType.Ordered &&
                msg.Header.Type != MsgType.ReliableOrdered)
            {
                base.ReceiveMsg(msg);
            }
            else Log.Warning(nameof(Peer), $"Received {msg.Header.Type} message on library receiver channel, message dropped");
        }
    }
}