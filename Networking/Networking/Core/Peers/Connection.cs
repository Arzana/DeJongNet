namespace DeJong.Networking.Core.Peers
{
    using Channels;
    using System.Net;
    using Channels.Sender;
    using Messages;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.Core;
    using Utilities.Logging;

    public sealed partial class Connection
    {
        public double Ping { get; private set; }
        public double AverageRoundTripTime { get; private set; }
        public ConnectionStatus Status { get; private set; }
        public NetID RemoteID { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        internal ReceiverController Receiver { get; private set; }
        private double lastPingSend;
        private int pingInterval;
        private int lastPingCount;

        private SenderController sender;
        private Queue<double> rttBuffer;
        private int pingCount;

        internal Connection(RawSocket socket, IPEndPoint remoteEP, PeerConfig config)
        {
            RemoteID = new NetID(NetUtils.GetID(remoteEP));
            RemoteEndPoint = remoteEP;
            rttBuffer = new Queue<double>();

            sender = new SenderController(socket, remoteEP, config);
            Receiver = new ReceiverController(remoteEP, (UnreliableSenderChannel)sender[0]);
            pingInterval = config.PingInterval;
            lastPingSend = NetTime.Now;
        }

        public override string ToString()
        {
            return RemoteID.ToString();
        }

        internal void PingConnection()
        {
            lastPingCount = pingCount = ExtraMath.Clamp(++pingCount, 0, int.MaxValue);
            SendTo(MessageHelper.Ping(pingCount), 0);
            lastPingSend = NetTime.Now;
        }

        internal void ReceivePong(IncommingMsg msg)
        {
            int pingNum = msg.ReadInt32();
            if (pingNum != lastPingCount) return;
            SetPing(msg.ReadSingle());
            AddRTT();
        }

        internal void Heartbeat()
        {
            if ((NetTime.Now - lastPingSend) >= pingInterval) PingConnection();

            Receiver.Heartbeat();
            sender.HeartBeat();
        }

        internal void ReceivePacket(RawSocket socket, PacketReceiveEventArgs e)
        {
            Receiver.ReceivedPacket(socket, e);
        }

        internal void SendTo(OutgoingMsg msg, int channel)
        {
            sender[channel].EnqueueMessage(msg);
        }

        internal void AddRTT()
        {
            Log.Debug(GetType().Name, $"{{Ping: {Ping}, ARTT: {AverageRoundTripTime}}}");

            double sec = NetTime.Now - lastPingSend;
            rttBuffer.Enqueue(sec / 0.001d);
            if (rttBuffer.Count > Constants.RTT_BUFFER_SIZE) rttBuffer.Dequeue();
            AverageRoundTripTime = rttBuffer.Average();
        }

        internal void SetPing(double send)
        {
            double sec = NetTime.Now - send;
            Ping = sec * 1000d;
        }
    }
}