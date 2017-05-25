namespace DeJong.Networking.Core.Channels.Sender
{
    using System.Net;
    using Messages;
    using System;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class LibSenderChannel : UnreliableSenderChannel
    {
        public LibSenderChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        { }

        public OutgoingMsg CreateMessage(MsgType type)
        {
            return new OutgoingMsg(ID, type, cache.Get());
        }

        public override OutgoingMsg CreateMessage()
        {
            RaiseChannelException();
            return null;
        }

        public override OutgoingMsg CreateMessage(int initialSize)
        {
            RaiseChannelException();
            return null;
        }

        private void RaiseChannelException()
        {
            LoggedException.Raise(nameof(LibSenderChannel), "Cannot create normal messages with library sender", new InvalidOperationException());
        }
    }
}
