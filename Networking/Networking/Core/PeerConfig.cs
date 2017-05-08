namespace DeJong.Networking.Core
{
    using Utilities.Core;
    using System.Runtime.CompilerServices;
    using System.Net;

    /// <summary>
    /// Defines how the library whould work.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
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
        /// Gets or sets the port that the socket should use (may differ from actual port).
        /// </summary>
        public int Port { get { return port; } set { CheckLock(); port = value; } }
        /// <summary>
        /// Gets or sets the size of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize { get { return receiveBufferSize; } set { CheckLock(); receiveBufferSize = value; } }
        /// <summary>
        /// Gets or sets the size of the send buffer.
        /// </summary>
        public int SendBufferSize { get { return sendBufferSize; } set { CheckLock(); sendBufferSize = value; } }

        private IPAddress localAddress;
        private int port;
        private int receiveBufferSize;
        private int sendBufferSize;
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
            ReceiveBufferSize = Constants.DEFAULT_BUFFER_SIZE;
            SendBufferSize = Constants.DEFAULT_BUFFER_SIZE;
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