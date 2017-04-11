namespace DeJong.Networking.Core.Connections
{
    using Channels;
    using System.Text;

    public sealed class NetConnectionStats
    {
        public long SendPackets { get; private set; }
        public long ReceivedPackets { get; private set; }
        public long SendBytes { get; private set; }
        public long ReceivedBytes { get; private set; }
        public long SendMessages { get; private set; }
        public long ReceivedMessages { get; private set; }
        public long ResendMessages { get { return resendMsgsDueToDelay + resendMsgsDueToHole; } }
        public long DroppedMessages { get; private set; }

        private readonly NetConnection connection;
        private long receivedFragments;
        private long resendMsgsDueToDelay;
        private long resendMsgsDueToHole;

        internal NetConnectionStats(NetConnection conn)
        {
            connection = conn;
            Reset();
        }

        internal void Reset()
        {
            SendPackets = 0;
            ReceivedPackets = 0;
            SendMessages = 0;
            ReceivedMessages = 0;
            SendBytes = 0;
            ReceivedBytes = 0;
            resendMsgsDueToDelay = 0;
            resendMsgsDueToHole = 0;
        }

        internal void PacketSend(int numBytes, int numMsgs)
        {
            NetException.RaiseIf(numBytes < 1 || numMsgs < 1);
            ++SendPackets;
            SendBytes += numBytes;
            SendMessages += numMsgs;
        }

        internal void PacketReceived(int numBytes, int numMsgs, int numFrags)
        {
            NetException.RaiseIf(numBytes < 1 || numMsgs < 0);
            ++ReceivedPackets;
            ReceivedBytes += numBytes;
            ReceivedMessages += numMsgs;
            receivedFragments += numFrags;
        }

        internal void MessageResend(MsgResendReason reason)
        {
            if (reason == MsgResendReason.Delay) ++resendMsgsDueToDelay;
            else ++resendMsgsDueToHole;
        }

        internal void MessageDropped()
        {
            ++DroppedMessages;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Current MUT: {connection.CurrentMTU}");
            sb.AppendLine($"Send {SendBytes} bytes in {SendMessages} messages in {SendPackets} packets");
            sb.AppendLine($"Received {ReceivedBytes} bytes in {ReceivedMessages} messages in {ReceivedPackets} packets");
            sb.AppendLine($"Dropped: {DroppedMessages} messages (duplicates, late or early)");
            if (resendMsgsDueToDelay > 0) sb.AppendLine($"Resend messages (delay): {resendMsgsDueToDelay}");
            if (resendMsgsDueToHole > 0) sb.AppendLine($"Resend messages (holes): {resendMsgsDueToHole}");

            int numUnsend = 0;
            int numStored = 0;
            foreach (NetSenderChannelBase sendChan in connection.sendChannels)
            {
                if (sendChan == null) continue;

                numUnsend += sendChan.QueuedSendsCount;
            }
        }

        internal enum MsgResendReason
        {
            Delay,
            HoleInSequence
        }
    }
}