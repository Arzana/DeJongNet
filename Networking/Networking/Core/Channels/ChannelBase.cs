namespace DeJong.Networking.Core.Channels
{
    using Utilities.Threading;
    using Messages;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal abstract class ChannelBase<T>
        where T : MsgBuffer
    {
        public int ID { get; set; }

        protected RawSocket socket;
        protected ThreadSafeQueue<T> queue;
        protected MessageCache cache;

        protected ChannelBase(RawSocket socket, PeerConfig config)
        {
            this.socket = socket;
            queue = new ThreadSafeQueue<T>();
            cache = new MessageCache(config);
        }

        public abstract void Heartbeat();

        public void Recycle(MsgBuffer msg)
        {
            if (msg == null) return;
            cache.Recycle(msg.data);
        }
    }
}