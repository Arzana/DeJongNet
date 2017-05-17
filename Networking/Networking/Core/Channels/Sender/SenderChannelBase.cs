namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System;
    using System.Net;
    using Utilities.Core;
    using Utilities.Threading;

    internal abstract class SenderChannelBase : ChannelBase<OutgoingMsg>
    {
        protected ThreadSafeList<Ack> sendPackets;

        private readonly IPEndPoint target;
        private WriteableBuffer packetWriteHelper;
        private int groupCount;

        protected SenderChannelBase(RawSocket socket, IPEndPoint target)
            : base(socket)
        {
            this.target = target;
            packetWriteHelper = new WriteableBuffer(socket.SendBuffer);
            sendPackets = new ThreadSafeList<Ack>();
        }

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

        private void SendMessage(OutgoingMsg msg)
        {
            bool send = true;

            LibHeader libHeader = msg.GenerateHeader(ID, Constants.MTU_ETHERNET_WITH_HEADERS);
            if (libHeader.Fragment)
            {
                int size = FragmentHeader.GetChunkSize(GetClampedGroupID(), msg.LengthBytes, Constants.MTU_ETHERNET_WITH_HEADERS);

                for (int i = 0, bytesLeft = msg.LengthBytes; bytesLeft > 0; i++, bytesLeft -= size)
                {
                    FragmentHeader fragHeader = new FragmentHeader(groupCount, msg.LengthBytes, size, i);
                    OutgoingMsg packet = new OutgoingMsg(libHeader.Type);
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
            packetWriteHelper.PositionBits = 0;

            libHeader.WriteToBuffer(packetWriteHelper);
            if (libHeader.Fragment) fragHeader.WriteToBuffer(packetWriteHelper);
            msg.CopyData(packetWriteHelper);

            bool connReset;
            socket.SendPacket(packetWriteHelper.LengthBytes + msg.LengthBytes, target, out connReset);
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