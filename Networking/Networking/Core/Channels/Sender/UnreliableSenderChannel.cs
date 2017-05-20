namespace DeJong.Networking.Core.Channels.Sender
{
    using System.Net;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class UnreliableSenderChannel : SenderChannelBase
    {
        public UnreliableSenderChannel(RawSocket socket, IPEndPoint remote)
            : base(socket, remote)
        { }

        public override void Heartbeat()
        {
            base.Heartbeat();
            sendPackets.Clear();
        }
    }
}