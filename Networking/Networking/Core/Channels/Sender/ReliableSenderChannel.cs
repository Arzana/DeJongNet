namespace DeJong.Networking.Core.Channels.Sender
{
    using Messages;
    using Peers;
    using Utilities.Core;

    internal sealed class ReliableSenderChannel : SenderChannelBase
    {
        private readonly int resendDelay;
        private int sequenceCount;

        public ReliableSenderChannel(RawSocket socket, Connection conn, PeerConfig config)
            : base(socket, conn.RemoteEndPoint)
        {
            resendDelay = config.ResendDelay;
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
            sendPackets.Remove(new Ack(new OutgoingMsg(MsgType.LibraryError) { SequenceNumber = sequnceNum }));
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