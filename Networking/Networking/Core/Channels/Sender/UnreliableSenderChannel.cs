namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System.Net;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal class UnreliableSenderChannel : SenderChannelBase
    {
        public UnreliableSenderChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        { }

        public override OutgoingMsg CreateMessage()
        {
            return new OutgoingMsg(ID, MsgType.Unreliable, cache.Get());
        }

        public override OutgoingMsg CreateMessage(int initialSize)
        {
            return new OutgoingMsg(ID, MsgType.Unreliable, cache.Get(initialSize));
        }

        public override void Heartbeat()
        {
            base.Heartbeat();
            sendPackets.Clear();
        }
    }
}