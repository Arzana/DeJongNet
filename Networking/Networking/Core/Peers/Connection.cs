namespace DeJong.Networking.Core.Peers
{
    using Channels;
    using System.Net;
    using Channels.Sender;
    using Messages;

    public sealed partial class Connection
    {
        public ConnectionStatus Status { get; private set; }
        public NetID RemoteID { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        private SenderController sender;
        private ReceiverController receiver;

        internal Connection(RawSocket socket, IPEndPoint remoteEP, PeerConfig config)
        {
            RemoteID = new NetID(NetUtils.GetID(remoteEP));
            RemoteEndPoint = remoteEP;

            sender = new SenderController(socket, remoteEP, config);
            receiver = new ReceiverController(remoteEP, (UnreliableSenderChannel)sender[0]);
        }

        internal void Heartbeat()
        {
            receiver.Heartbeat();
            sender.HeartBeat();
        }

        internal void ReceivePacket(RawSocket socket, PacketReceiveEventArgs e)
        {
            receiver.ReceivedPacket(socket, e);
        }

        internal void SendTo(OutgoingMsg msg, int channel)
        {
            sender[channel].EnqueueMessage(msg);
        }
    }
}