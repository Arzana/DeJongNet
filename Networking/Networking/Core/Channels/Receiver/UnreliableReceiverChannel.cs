namespace DeJong.Networking.Core.Channels.Receiver
{
    using System.Net;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class UnreliableReceiverChannel : ReceiverChannelBase
    {
        public UnreliableReceiverChannel(IPEndPoint remote)
            : base(remote)
        { }
    }
}