namespace DeJong.Networking.Core.Peers
{
    using Channels;
    using Channels.Sender;
    using Utilities.Logging;
    using Messages;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed partial class Connection
    {
        public double Ping { get; private set; }
        public double AverageRoundTripTime { get; private set; }
        public ConnectionStatus Status { get; internal set; }
        public NetID RemoteID { get; internal set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        internal ReceiverController Receiver { get; private set; }
        internal SenderController Sender { get; private set; }

        private double lastPingSend;
        private int pingInterval;
        private int lastPingCount;
        private Queue<double> rttBuffer;
        private int pingCount;

        internal Connection(RawSocket socket, IPEndPoint remoteEP, PeerConfig config)
        {
            RemoteID = NetID.Unknown;
            RemoteEndPoint = remoteEP;
            rttBuffer = new Queue<double>();

            Sender = new SenderController(socket, remoteEP, config);
            Receiver = new ReceiverController(socket, remoteEP, config, Sender);
            pingInterval = config.PingInterval;
            lastPingSend = NetTime.Now;
        }

        public override string ToString()
        {
            return $"{RemoteID} ({Status})";
        }

        internal void Disconnect(string reason)
        {
            Status = ConnectionStatus.Disconnected;
            Log.Verbose(nameof(Connection), $"Disconnected from remote host {RemoteID}, {reason}");
        }

        internal void PingConnection()
        {
            lastPingCount = pingCount = ExtraMath.Clamp(++pingCount, 0, int.MaxValue);
            SendTo(MessageHelper.Ping(Sender.LibSender.CreateMessage(MsgType.Ping), pingCount));
            lastPingSend = NetTime.Now;
        }

        internal void ReceivePong(IncommingMsg msg)
        {
            int pingNum = msg.ReadInt32();
            if (pingNum != lastPingCount)
            {
                Log.Warning(nameof(Connection), $"Received {(pingNum < lastPingCount ? "late" : "unknown")} pong from {RemoteID}");
            }
            else
            {
                SetPing(msg.ReadSingle());
                AddRTT();
            }
        }

        internal void Heartbeat()
        {
            if ((NetTime.Now - lastPingSend) >= pingInterval) PingConnection();

            Receiver.Heartbeat();
            Sender.HeartBeat();
        }

        internal void ReceivePacket(RawSocket socket, PacketReceiveEventArgs e)
        {
            Receiver.ReceivedPacket(socket, e);
        }

        internal void SendTo(OutgoingMsg msg)
        {
            Sender[msg.channel].EnqueueMessage(msg);
        }

        internal void AddRTT()
        {
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