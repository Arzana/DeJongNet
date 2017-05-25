namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using Peers;
    using System.Net;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class UnreliableReceiverChannel : ReceiverChannelBase
    {
        public UnreliableReceiverChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        { }

        protected override void ReceiveMsg(IncommingMsg msg)
        {
            if (msg.Header.Type == MsgType.Unreliable) base.ReceiveMsg(msg);
            else Log.Warning(nameof(Peer), $"Received {msg.Header.Type} message on unreliable receiver channel, message dropped");
        }
    }
}