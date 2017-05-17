namespace DeJong.Networking.Core.Channels.Receiver
{
    using Peers;

    internal sealed class UnreliableReceiverChannel : ReceiverChannelBase
    {
        public UnreliableReceiverChannel(RawSocket socket, Connection conn)
            : base(socket, conn.RemoteEndPoint)
        { }
    }
}