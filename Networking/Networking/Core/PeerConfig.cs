namespace DeJong.Networking.Core
{
    using Utilities.Core;
    using System.Runtime.CompilerServices;
    using System.Net;
    using System.Diagnostics;

    /// <summary>
    /// Defines how the library whould work.
    /// </summary>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("Config for {AppID}")]
    public sealed class PeerConfig
    {
        /// <summary>
        /// Gets a string that serves as an indentifier for the peer.
        /// </summary>
        public string AppID { get; private set; }
        /// <summary>
        /// Gets or sets the local address.
        /// </summary>
        public IPAddress LocalAddress { get { return localAddress; } set { CheckLock(); localAddress = value; } }
        /// <summary>
        /// Gets or sets how big the buffer for the message cache can be.
        /// </summary>
        public int MessageCacheSize { get { return maxBufferSize; } set { CheckLock(); maxBufferSize = value; } }
        /// <summary>
        /// Gets or sets the maximum transmision unit.
        /// </summary>
        public int MTU { get; set; }
        /// <summary>
        /// Gets or sets a value indicating the name of the underlying networking thread.
        /// </summary>
        public string NetworkThreadName { get { return networkThreadName; } set { CheckLock(); networkThreadName = value; } }
        /// <summary>
        /// Gets or sets a value indicating the time (in seconds) between latency calculations.
        /// </summary>
        public int PingInterval { get; set; }
        /// <summary>
        /// Gets or sets the port that the socket should use (may differ from actual port).
        /// </summary>
        public int Port { get { return port; } set { CheckLock(); port = value; } }
        /// <summary>
        /// Gets or sets the size of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize { get { return receiveBufferSize; } set { CheckLock(); receiveBufferSize = value; } }
        /// <summary>
        /// Gets or sets a value indicating the delay (in seconds) before resending a reliable message.
        /// </summary>
        public int ResendDelay { get { return resendDelay; } set { CheckLock(); resendDelay = value; } }
        /// <summary>
        /// Gets or sets the size of the send buffer.
        /// </summary>
        public int SendBufferSize { get { return sendBufferSize; } set { CheckLock(); sendBufferSize = value; } }

        private static int peersCreated;

        private IPAddress localAddress;
        private int port;
        private int receiveBufferSize;
        private int sendBufferSize;
        private int resendDelay;
        private string networkThreadName;
        private int maxBufferSize;
        private bool locked;

        /// <summary>
        /// Creates a new instance of the <see cref="PeerConfig"/> class with default values.
        /// </summary>
        /// <param name="id"> The app indentifier. </param>
        public PeerConfig(string id)
        {
            LoggedException.RaiseIf(string.IsNullOrEmpty(id), nameof(PeerConfig), $"{nameof(AppID)} cannot be empty");
            AppID = id;

            LocalAddress = IPAddress.Any;
            MTU = Constants.MTU_ETHERNET;
            ReceiveBufferSize = Constants.DEFAULT_BUFFER_SIZE;
            SendBufferSize = Constants.DEFAULT_BUFFER_SIZE;
            ResendDelay = Constants.DEFAULT_RESEND_DELAY;
            PingInterval = Constants.DEFAULT_PING_INTERVAL;
            NetworkThreadName = "DeJong Networking";
            MessageCacheSize = Constants.DEFAULT_CACHE_SIZE;
            if (++peersCreated > 1) NetworkThreadName += $" {peersCreated}";
        }

        internal void Lock()
        {
            locked = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckLock()
        {
            LoggedException.RaiseIf(locked, nameof(PeerConfig), $"You may not modify the {nameof(PeerConfig)} after it has been used to initialize a NetPeer");
        }
    }
}