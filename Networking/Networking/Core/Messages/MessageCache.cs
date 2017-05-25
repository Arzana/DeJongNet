namespace DeJong.Networking.Core.Messages
{
    using System;
    using System.Diagnostics;
    using Utilities.Core;
    using Utilities.Threading;

    [DebuggerDisplay("{ToDebbugerString()}")]
    internal sealed class MessageCache : IFullyDisposable
    {
        public bool Disposed { get; private set; }
        public bool Disposing { get; private set; }

        private ThreadSafeList<byte[]> cache;
        private int maxBufferSize;
        private int maxCachedBuffers;

        public MessageCache(PeerConfig config)
        {
            cache = new ThreadSafeList<byte[]>();
            maxCachedBuffers = config.MessageCacheSize;
        }

        ~MessageCache()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!(Disposed | Disposing))
            {
                Disposing = true;
                cache.Dispose();
                Disposed = true;
                Disposing = false;
            }
        }

        public byte[] Get()
        {
            return Get(maxBufferSize);
        }

        public byte[] Get(int minimumSize)
        {
            for (int i = 0; i < cache.Count; i++)
            {
                if (cache[i].Length >= minimumSize)
                {
                    byte[] result = cache[i];
                    cache.RemoveAt(i);
                    return result;
                }
            }

            return new byte[minimumSize];
        }

        public void Recycle(byte[] buffer)
        {
            if (buffer.Length > maxBufferSize) maxBufferSize = buffer.Length;

            if (cache.Count < maxCachedBuffers) ResizeAdd(buffer);
            else
            {
                for (int i = 0; i < cache.Count; i++)
                {
                    if (cache[i].Length < buffer.Length)
                    {
                        cache.RemoveAt(i);
                        ResizeAdd(buffer);
                    }
                }
            }
        }

        private void ResizeAdd(byte[] buffer)
        {
            if (buffer.Length < maxCachedBuffers) Array.Resize(ref buffer, maxBufferSize);
            Array.Clear(buffer, 0, buffer.Length);
            cache.Add(buffer);
        }

        private string ToDebbugerString()
        {
            return $"{cache.Count}/{maxCachedBuffers} buffers of size {maxBufferSize}";
        }
    }
}