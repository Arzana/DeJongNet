namespace DeJong.Networking.Core.Channels
{
    using Utilities.Threading;
    using Messages;

    internal abstract class ChannelBase<T>
        where T : MsgBuffer
    {
        public int ID { get; set; }

        protected ThreadSafeQueue<T> queue;
        protected RawSocket socket;

        protected ChannelBase(RawSocket socket)
        {
            this.socket = socket;
            queue = new ThreadSafeQueue<T>();
        }

        public abstract void Heartbeat();
    }
}