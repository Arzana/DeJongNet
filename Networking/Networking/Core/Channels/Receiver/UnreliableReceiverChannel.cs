namespace DeJong.Networking.Core.Channels.Receiver
{
    using System.Net;

    internal sealed class UnreliableReceiverChannel : ReceiverChannelBase
    {
        public UnreliableReceiverChannel(IPEndPoint remote)
            : base(remote)
        { }
    }
}