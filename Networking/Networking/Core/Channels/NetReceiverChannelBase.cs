namespace DeJong.Networking.Core.Channels
{
    using Connections;
    using Msg;
    using NetPeer;

    internal abstract class NetReceiverChannelBase
    {
        internal NetPeer peer;
        internal NetConnection connection;

        public NetReceiverChannelBase(NetConnection connection)
        {
            this.connection = connection;
            peer = connection.Peer;
        }

        internal abstract void ReceiveMsg(NetIncomingMsg msg);
    }
}
