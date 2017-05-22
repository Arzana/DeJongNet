namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using System.Net;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal class ReliableSenderChannel : SenderChannelBase
    {
        private readonly int resendDelay;
        private int sequenceCount;

        public ReliableSenderChannel(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, remote, config)
        {
            resendDelay = config.ResendDelay;
        }

        public override OutgoingMsg CreateMessage()
        {
            return new OutgoingMsg(ID, MsgType.Reliable, cache.Get());
        }

        public override OutgoingMsg CreateMessage(int initialSize)
        {
            return new OutgoingMsg(ID, MsgType.Reliable, cache.Get(initialSize));
        }

        public override void Heartbeat()
        {
            for (int i = 0; i < sendPackets.Count; i++)
            {
                Ack cur = sendPackets[i];

                if (!cur.Msg.IsSend) queue.Enqueue(cur.Msg);
                else if ((NetTime.Now - cur.TimeSend) >= resendDelay)
                {
                    cur.Msg.IsSend = false;
                    queue.Enqueue(cur.Msg);
                }
            }

            base.Heartbeat();
        }

        public void ReceiveAck(int sequnceNum)
        {
            sendPackets.Remove(new Ack(new OutgoingMsg(ID, MsgType.LibraryError, null) { SequenceNumber = sequnceNum }));
        }

        public override void EnqueueMessage(OutgoingMsg msg)
        {
            msg.SequenceNumber = GetClampSequenceCount();
            base.EnqueueMessage(msg);
        }

        private int GetClampSequenceCount()
        {
            return sequenceCount = ExtraMath.Clamp(++sequenceCount, 0, 1024);
        }
    }
}