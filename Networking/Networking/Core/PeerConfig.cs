namespace DeJong.Networking.Core
{
    using Utilities.Core;
    using System.Runtime.CompilerServices;
    using System.Net;
    using System.Diagnostics;
    using Messages;

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
        /// Gets or sets a value indicating the maximum amount of connections the peer may have at one time.
        /// </summary>
        public int MaximumConnections { get { return maxConnections; } set { CheckLock(); maxConnections = value; } }
        /// <summary>
        /// Gets or sets how big the buffer for the message cache can be.
        /// </summary>
        public int MessageCacheSize { get { return maxBufferSize; } set { CheckLock(); maxBufferSize = value; } }
        /// <summary>
        /// Gets or sets the maximum transmision unit.
        /// </summary>
        public int MTU
        {
            get { return mtu; }
            set
            {
                CheckLock();
                const int MIN = LibHeader.SIZE_BYTES + FragmentHeader.SIZE_BYTES;
                const int MAX = (ushort.MaxValue + 1) / 8;
                LoggedException.RaiseIf(value < MIN || value > MAX, nameof(PeerConfig), $"Value must be between {MIN} and {MAX}");
                mtu = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating the name of the underlying networking thread.
        /// </summary>
        public string NetworkThreadName { get { return networkThreadName; } set { CheckLock(); networkThreadName = value; } }
        /// <summary>
        /// Gets or sets a value indicating the time (in seconds) between latency calculations.
        /// </summary>
        public float PingInterval
        {
            get { return pingInterval; }
            set
            {
                CheckLock();
                LoggedException.RaiseIf(value >= ConnectionTimeout, nameof(PeerConfig), "Value cannot be greater or equal to the connection timeout");
                pingInterval = value;
            }
        }
        /// <summary>
        /// Gets or sets the port that the socket should use (may differ from actual port).
        /// </summary>
        public int Port { get { return port; } set { CheckLock(); port = value; } }
        /// <summary>
        /// Gets or sets the size of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize
        {
            get { return receiveBufferSize; }
            set
            {
                CheckLock();
                LoggedException.RaiseIf(value < 1, nameof(PeerConfig), "Value cannot be smaller than one byte");
                receiveBufferSize = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating the delay (in seconds) before resending a reliable message.
        /// </summary>
        public int ResendDelay { get { return resendDelay; } set { CheckLock(); resendDelay = value; } }
        /// <summary>
        /// Gets or sets the size of the send buffer.
        /// </summary>
        public int SendBufferSize
        {
            get { return sendBufferSize; }
            set
            {
                CheckLock();
                LoggedException.RaiseIf(value < 1, nameof(PeerConfig), "Value cannot be smaller than one byte");
                sendBufferSize = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating the delay (in seconds) before the connection gets timed out when no messages are received.
        /// </summary>
        public float ConnectionTimeout
        {
            get { return timeout; }
            set
            {
                CheckLock();
                LoggedException.RaiseIf(value <= PingInterval, nameof(PeerConfig), "Value cannot be smaller or equal to the ping interval");
                timeout = value;
            }
        }

        private static int peersCreated;

        private IPAddress localAddress;
        private int port;
        private int receiveBufferSize;
        private int sendBufferSize;
        private int resendDelay;
        private string networkThreadName;
        private float pingInterval;
        private int maxBufferSize;
        private int maxConnections;
        private float timeout;
        private int mtu;
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
            MTU = Constants.MTU_ETHERNET_WITH_HEADERS;
            ReceiveBufferSize = 131071;
            SendBufferSize = 131071;
            ResendDelay = 2;
            ConnectionTimeout = 25;
            PingInterval = 4;
            NetworkThreadName = "DeJong Networking";
            MessageCacheSize = 10;
            maxConnections = 25;
            if (++peersCreated > 1) NetworkThreadName += $" {peersCreated}";
        }

        /// <summary>
        /// Creates a clone of this configuration that isn't locked.
        /// </summary>
        /// <returns></returns>
        public PeerConfig Clone()
        {
            PeerConfig result = MemberwiseClone() as PeerConfig;
            result.locked = false;
            return result;
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