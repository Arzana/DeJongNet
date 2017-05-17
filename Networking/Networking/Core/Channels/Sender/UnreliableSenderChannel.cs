namespace DeJong.Networking.Core.Channels.Sender
{
    using Peers;

    internal sealed class UnreliableSenderChannel : SenderChannelBase
    {
        public UnreliableSenderChannel(RawSocket socket, Connection conn)
            : base(socket, conn.RemoteEndPoint)
        { }

        public override void Heartbeat()
        {
            base.Heartbeat();
            sendPackets.Clear();
        }
    }
}