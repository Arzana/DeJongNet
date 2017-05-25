namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System;
    using System.Net;
    using Utilities.Core;
    using Utilities.Threading;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal abstract class SenderChannelBase : ChannelBase<OutgoingMsg>
    {
        protected ThreadSafeList<Ack> sendPackets;

        private readonly IPEndPoint target;
        private WriteableBuffer packetWriteHelper;
        private int groupCount;
        private PeerConfig config;

        protected SenderChannelBase(RawSocket socket, IPEndPoint target, PeerConfig config)
            : base(socket, config)
        {
            this.target = target;
            this.config = config;
            packetWriteHelper = new WriteableBuffer(socket.SendBuffer);
            sendPackets = new ThreadSafeList<Ack>();
        }

        public abstract OutgoingMsg CreateMessage();
        public abstract OutgoingMsg CreateMessage(int initialSize);

        public override void Heartbeat()
        {
            while (queue.Count > 0)
            {
                SendMessage(queue.Dequeue());
            }
        }

        public virtual void EnqueueMessage(OutgoingMsg msg)
        {
            queue.Enqueue(msg);
        }

        public override string ToString()
        {
            return $"Sender channel {ID}";
        }

        private void SendMessage(OutgoingMsg msg)
        {
            LoggedException.RaiseIf(msg.IsSend, nameof(SenderChannelBase), "Message already send");
            bool send = true;

            LibHeader libHeader = msg.GenerateHeader(config.MTU);
            if (libHeader.Fragment)
            {
                int size = FragmentHeader.GetChunkSize(GetClampedGroupID(), msg.LengthBytes, config.MTU);

                for (int i = 0, bytesLeft = msg.LengthBytes; bytesLeft > 0; i++, bytesLeft -= size)
                {
                    FragmentHeader fragHeader = new FragmentHeader(groupCount, msg.LengthBytes, size, i);
                    OutgoingMsg packet = CreateMessage();
                    msg.CopyData(packet, msg.LengthBytes - bytesLeft, size);

                    send = send && SendPacket(libHeader, fragHeader, packet);
                }
            }
            else send = SendPacket(libHeader, FragmentHeader.Empty, msg);

            msg.IsSend = send;
            sendPackets.Add(new Ack(msg));
        }

        private bool SendPacket(LibHeader libHeader, FragmentHeader fragHeader, OutgoingMsg msg)
        {
            packetWriteHelper.LengthBits = 0;

            libHeader.WriteToBuffer(packetWriteHelper);
            if (libHeader.Fragment) fragHeader.WriteToBuffer(packetWriteHelper);
            msg.CopyData(packetWriteHelper);

            bool connReset;
            socket.SendPacket(packetWriteHelper.LengthBytes + msg.LengthBytes, target, out connReset);
            if (!connReset) Recycle(msg);

            return !connReset;
        }

        private int GetClampedGroupID()
        {
            return groupCount = ExtraMath.Clamp(++groupCount, 0, short.MaxValue);
        }

        protected struct Ack : IEquatable<Ack>
        {
            public readonly OutgoingMsg Msg;
            public readonly double TimeSend;

            public static bool operator ==(Ack left, Ack right) { return left.Equals(right); }
            public static bool operator !=(Ack left, Ack right) { return !left.Equals(right); }

            public static bool operator ==(Ack left, int right) { return left.Msg.SequenceNumber == right; }
            public static bool operator !=(Ack left, int right) { return left.Msg.SequenceNumber == right; }

            public static bool operator ==(int left, Ack right) { return right.Msg.SequenceNumber == left; }
            public static bool operator !=(int left, Ack right) { return right.Msg.SequenceNumber == left; }

            public Ack(OutgoingMsg msg)
            {
                Msg = msg;
                TimeSend = NetTime.Now;
            }

            public override bool Equals(object obj)
            {
                return obj.GetType() == typeof(Ack) ? Equals((Ack)obj) : false;
            }

            public bool Equals(Ack other)
            {
                return other.Msg.SequenceNumber == Msg.SequenceNumber;
            }

            public override int GetHashCode()
            {
                return Msg.SequenceNumber.GetHashCode();
            }

            public override string ToString()
            {
                return $"{Msg.SequenceNumber}";
            }
        }
    }
}