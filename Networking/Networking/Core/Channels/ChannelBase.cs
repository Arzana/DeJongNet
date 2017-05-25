namespace DeJong.Networking.Core.Channels
{
    using Utilities.Threading;
    using Messages;
    using Utilities.Core;
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal abstract class ChannelBase<T> : IFullyDisposable
        where T : MsgBuffer
    {
        public int ID { get; set; }
        public bool Disposed { get; private set; }
        public bool Disposing { get; private set; }

        protected RawSocket socket;
        protected ThreadSafeQueue<T> queue;
        protected MessageCache cache;

        protected ChannelBase(RawSocket socket, PeerConfig config)
        {
            this.socket = socket;
            queue = new ThreadSafeQueue<T>();
            cache = new MessageCache(config);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!(Disposed | Disposing))
            {
                Disposing = true;
                socket.Dispose();
                queue.Dispose();
                cache.Dispose();
                Disposed = true;
                Disposing = false;
            }
        }

        public abstract void Heartbeat();

        public void Recycle(MsgBuffer msg)
        {
            if (msg == null) return;
            cache.Recycle(msg.data);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}