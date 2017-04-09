using System.Net;

namespace Mentula.Networking.Core
{
    /// <summary>
    /// Defines configurable properties for a net peer.
    /// </summary>
    /// <remarks>
    /// Partly immutable after net peer has been initialized.
    /// </remarks>
    public sealed class NetPeerConfig
    {
        /// <summary>
        /// Default MTU value in bytes.
        /// </summary>
        public const int DEFUALT_MTU = 1408;

        /// <summary>
        /// Gets the indentifier of this application.
        /// </summary>
        public string AppID { get; private set; }
        /// <summary>
        /// Gets or sets the name of the networking thread.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = "Mentula networking thread".
        /// </remarks>
        public string NetWorkThreadName { get { return networkingThreadName; } set { CheckLock(); networkingThreadName = value; } }
        /// <summary>
        /// Gets or sets the local ip address to bind to.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see cref="IPAddress.Any"/>.
        /// </remarks>
        public IPAddress LocalAddress { get { return localAddress; } set { CheckLock(); localAddress = value; } }
        /// <summary>
        /// Gets or sets the local broadcast address to use when broadcasting.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see cref="IPAddress.Broadcast"/>.
        /// </remarks>
        public IPAddress BroadcastAddress { get { return broadcastAddress; } set { CheckLock(); broadcastAddress = value; } }
        /// <summary>
        /// Gets or sets whether the net peer should accept incoming connections.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> for net servers,
        /// <see langword="false"/> for net clients.
        /// </remarks>
        public bool AcceptsIncomingConnections { get; set; }
        /// <summary>
        /// Gets or sets the maximum amount of connections this peer can hold.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = 32.
        /// </remarks>
        public int MaximumConnections { get { return maxConnections; } set { CheckLock(); maxConnections = value; } }
        /// <summary>
        /// Gets or set the default capacity in bytes when a new message is created without arguments.
        /// </summary>
        /// <remarks>
        /// Default value = 16.
        /// </remarks>
        public int DefaulOutgoingMessageCapacity { get; set; }
        /// <summary>
        /// Gets or sets the time between latency calculating pings.
        /// </summary>
        /// <remarks>
        /// Default value = 4.
        /// </remarks>
        public float PingInterval { get; set; }
        /// <summary>
        /// Gets or sets if messages should recycled to avoid excessive garbage collection.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see langword="true"/>.
        /// </remarks>
        public bool UseMessageRecycling { get { return useMsgRecycling; } set { CheckLock(); useMsgRecycling = value; } }
        /// <summary>
        /// Gets or sets the maximum number of incoming and outgoing messages to keep in the recycle cache.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = 64.
        /// </remarks>
        public int MaxRecycledCacheCount { get { return recyledCacheMaxCount; } set { CheckLock(); recyledCacheMaxCount = value; } }
        /// <summary>
        /// Gets or sets the number of seconds timeout will be postponed on a successfull ping or pong.
        /// </summary>
        /// <remarks>
        /// Default value = 25.
        /// </remarks>
        public float ConnectionTimeout
        {
            get { return connectionTimeout; }
            set
            {
                NetException.RaiseIf(value < PingInterval, "Connection timeout cannot be lower than ping interval!");
                connectionTimeout = value;
            }
        }
        /// <summary>
        /// Gets or sets UPnP; enabling or disabling port forwarding and getting external ip.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see langword="false"/>.
        /// </remarks>
        public bool EnableUPnP { get { return enableUPnP; } set { CheckLock(); enableUPnP = value; } }
        /// <summary>
        /// Gets or sets a value indicating whether to automaticly flush the send queue.
        /// </summary>
        /// <remarks>
        /// Default value = <see langword="true"/>.
        /// </remarks>
        public bool AutoFlushSendQueue { get; set; }
        public NetUnreliableSizeBehaviour UnreliableSizeBehaviour { get; set; }
        public bool SuppressUnreliableUnorderedAcks { get { return suppressUnreliableUnordederAcks; } set { CheckLock(); suppressUnreliableUnordederAcks = value; } }
        public int Port { get { return port; } set { CheckLock(); port = value; } }
        public int ReceiveBufferSize { get { return receiveBufferSize; } set { CheckLock(); receiveBufferSize = value; } }
        public int SendBufferSize { get { return sendBufferSize; } set { CheckLock(); sendBufferSize = value; } }
        public float ResendHandshakeInterval { get; set; }
        public int MaxHandshakeAttempts
        {
            get { return handshakeAttemptsMax; }
            set
            {
                NetException.RaiseIf(value < 1, $"{nameof(MaxHandshakeAttempts)} must be at least 1!");
                handshakeAttemptsMax = value;
            }
        }

        public float SimulateLoss { get; set; }
        public float SimulateDuplicatesChance { get; set; }
        public float SimulateMinimumLatency { get; set; }
        public float SimulateRandomLatency { get; set; }
        public int MaximumTransMissionUnit
        {
            get { return maxTransmissionUnit; }
            set
            {
                CheckLock();
                NetException.RaiseIf(value < 1 || value >= (ushort.MaxValue + 1) / 8, $"{nameof(MaximumTransMissionUnit)} must be between 1 and {(ushort.MaxValue + 1) / 8 - 1} bytes!");
                maxTransmissionUnit = value;
            }
        }
        public bool AutoExpandMTU { get { return autoExpandMTU; } set { CheckLock(); autoExpandMTU = value; } }
        public float ExpandMTUFrequency { get; set; }
        public int ExpandMTUFailAttempts { get; set; }

        private bool isLocked;
        private string networkingThreadName;
        private IPAddress localAddress;
        private IPAddress broadcastAddress;
        private int maxConnections;
        private bool useMsgRecycling;
        private int recyledCacheMaxCount;
        private float connectionTimeout;
        private bool enableUPnP;
        private bool suppressUnreliableUnordederAcks;
        private NetIncomingMessageType disabledTypes;
        private int port;
        private int receiveBufferSize;
        private int sendBufferSize;
        private int handshakeAttemptsMax;
        private int maxTransmissionUnit;
        private bool autoExpandMTU;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetPeerConfig"/> class with a specified appication id.
        /// </summary>
        /// <param name="id"> The application id. </param>
        public NetPeerConfig(string id)
        {
            NetException.RaiseIf(string.IsNullOrEmpty(id), "App indentifier cannot be empty!");
            AppID = id;

            disabledTypes = NetIncomingMessageType.ConnectionApproval | NetIncomingMessageType.UnconnectedData | NetIncomingMessageType.ConnectionLatencyUpdate | NetIncomingMessageType.NatIntroductionSuccess;
            networkingThreadName = "Mentula networking thread";
            localAddress = IPAddress.Any;
            broadcastAddress = IPAddress.Broadcast;
            receiveBufferSize = 131071;
            sendBufferSize = 131071;
            maxConnections = 32;
            DefaulOutgoingMessageCapacity = 16;
            PingInterval = 4;
            connectionTimeout = 25;
            useMsgRecycling = true;
            recyledCacheMaxCount = 64;
            ResendHandshakeInterval = 3;
            handshakeAttemptsMax = 5;
            AutoFlushSendQueue = true;

            maxTransmissionUnit = DEFUALT_MTU;
            ExpandMTUFrequency = 2;
            ExpandMTUFailAttempts = 5;
        }

        public void EnableMessageType(NetIncomingMessageType type)
        {
            disabledTypes &= (~type);
        }

        public void DisableMessageType(NetIncomingMessageType type)
        {
            disabledTypes |= type;
        }

        public void SetMessageType(NetIncomingMessageType type, bool enabled)
        {
            if (enabled) EnableMessageType(type);
            else DisableMessageType(type);
        }

        public bool IsMessageTypeEnabled(NetIncomingMessageType type)
        {
            return (disabledTypes & type) != type;
        }

        public NetPeerConfig Clone()
        {
            NetPeerConfig result = MemberwiseClone() as NetPeerConfig;
            result.isLocked = false;
            return result;
        }

        internal void Lock()
        {
            isLocked = true;
        }

        private void CheckLock()
        {
            NetException.RaiseIf(isLocked, $"{nameof(NetPeerConfig)} cannot be modified after a NetPeer has been initialized!");
        }
    }
}
