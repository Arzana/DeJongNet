namespace DeJong.Networking.Core.Channels
{
    using Msg;
    using Utilities.Threading;

    internal abstract class NetSenderChannelBase
    {
        internal int QueuedSendsCount { get { return queuedSends.Count; } }
        internal abstract int WindowSize { get; }

        protected ThreadSafeQueue<NetOutgoingMsg> queuedSends;

        public int GetFreeWindowSlots()
        {
            return GetAllowedSends() - QueuedSendsCount;
        }

        internal virtual bool NeedToSendMsgs()
        {
            return QueuedSendsCount > 0;
        }

        internal abstract int GetAllowedSends();
        internal abstract void SendQueuedMsgs(double now);
        internal abstract void Reset();
        internal abstract void ReceiveAcknowledge(double now, int sequenceNum);
    }
}
