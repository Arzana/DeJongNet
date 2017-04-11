namespace DeJong.Networking.Core.Connections
{
    using Channels;

    public partial class NetConnection
    {
        public NetPeer.NetPeer Peer { get; private set; }

        internal NetSenderChannelBase[] sendChannels;
        internal NetReceiverChannelBase[] receiveChannels;
    }
}