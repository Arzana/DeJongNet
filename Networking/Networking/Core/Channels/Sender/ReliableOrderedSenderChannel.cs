namespace DeJong.Networking.Core.Channels.Sender
{
    using System.Net;
    using Messages;

    internal sealed class ReliableOrderedSenderChannel : ReliableSenderChannel
    {
        public ReliableOrderedSenderChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            :base(socket, remote, config)
        { }

        public override OutgoingMsg CreateMessage()
        {
            return new OutgoingMsg(ID, MsgType.ReliableOrdered, cache.Get());
        }

        public override OutgoingMsg CreateMessage(int initialSize)
        {
            return new OutgoingMsg(ID, MsgType.ReliableOrdered, cache.Get(initialSize));
        }
    }
}