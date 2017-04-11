namespace DeJong.Networking.Core.NetPeer
{
    using Msg;
    using System.Diagnostics;
    using System.Net;

    /// <summary>
    /// Defines configurable properties for a net peer.
    /// </summary>
    /// <remarks>
    /// Partly immutable after net peer has been initialized.
    /// </remarks>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("Config for: {AppID}")]
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
        /// <summary>
        /// Gets or set the behaviour of unreliable sends above MTU.
        /// </summary>
        /// <remarks>
        /// Default value = <see cref="NetUnreliableSizeBehaviour.IgnoreMTU"/>.
        /// </remarks>
        public NetUnreliableSizeBehaviour UnreliableSizeBehaviour { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether acks of unreliable unordered sends should be suppressed.
        /// </summary>
        /// <remarks>
        /// If set to <see langword="true"/> this will not send acks for unreliable unordered messages.
        /// This will save bandwidth, but disable flow control and duplicate detection for this message type.
        /// Immutable after net peer initialization.
        /// Default value = <see langword="false"/>.
        /// </remarks>
        public bool SuppressUnreliableUnorderedAcks { get { return suppressUnreliableUnordederAcks; } set { CheckLock(); suppressUnreliableUnordederAcks = value; } }
        /// <summary>
        /// Gets or sets a value indicating the port to bind to.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = 0.
        /// </remarks>
        public int Port { get { return port; } set { CheckLock(); port = value; } }
        /// <summary>
        /// Gets or sets the size in bytes of the message receive buffer.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = 131071
        /// </remarks>
        public int ReceiveBufferSize { get { return receiveBufferSize; } set { CheckLock(); receiveBufferSize = value; } }
        /// <summary>
        /// Gets or sets the size in bytes of the message sending buffer.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = 131071
        /// </remarks>
        public int SendBufferSize { get { return sendBufferSize; } set { CheckLock(); sendBufferSize = value; } }
        /// <summary>
        /// Gets or sets the amount of seconds between handshake attempts.
        /// </summary>
        /// <remarks>
        /// Default value = 3.
        /// </remarks>
        public float ResendHandshakeInterval { get; set; }
        /// <summary>
        /// Gets or sets the maximum amount of handshake attempts before failing to connect.
        /// </summary>
        /// <remarks>
        /// Default value = 5.
        /// </remarks>
        public int MaxHandshakeAttempts
        {
            get { return handshakeAttemptsMax; }
            set
            {
                NetException.RaiseIf(value < 1, $"{nameof(MaxHandshakeAttempts)} must be at least 1!");
                handshakeAttemptsMax = value;
            }
        }

        /// <summary>
        /// Gets or sets the simulated amount (0 to 1) of package loss.
        /// </summary>
        /// <remarks>
        /// Default value = 0.
        /// </remarks>
        public float SimulateLoss { get; set; }
        /// <summary>
        /// Gets or sets the simulated amount (0 to 1) of duplicate packages.
        /// </summary>
        public float SimulateDuplicatesChance { get; set; }
        /// <summary>
        /// Gets or sets the minimum simulated amount of one way latency for sent packets in seconds.
        /// </summary>
        public float SimulateMinimumLatency { get; set; }
        /// <summary>
        /// Gets or sets the simulated added random amount of one way latency for sent packets in seconds.
        /// </summary>
        public float SimulateRandomLatency { get; set; }
        /// <summary>
        /// Gets the average simulated one way latency in seconds.
        /// </summary>
        public float SimulatedAverageLatency { get { return SimulateMinimumLatency + SimulateRandomLatency * 0.5f; } }
        /// <summary>
        /// Gets or sets the maximum amount of bytes to send in a single packet, excluding ip, udp and mentula headers.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see cref="DEFUALT_MTU"/>.
        /// </remarks>
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
        /// <summary>
        /// Gets or sets if the net peer should send large messages to try to expand the maximum transmission unit size.
        /// </summary>
        /// <remarks>
        /// Immutable after net peer initialization.
        /// Default value = <see langword="false"/>.
        /// </remarks>
        public bool AutoExpandMTU { get { return autoExpandMTU; } set { CheckLock(); autoExpandMTU = value; } }
        /// <summary>
        /// Gets or sets how often to send large messages to expand MTU if <see cref="AutoExpandMTU"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Default value = 2.
        /// </remarks>
        public float ExpandMTUFrequency { get; set; }
        /// <summary>
        /// Gets or sets the number of failed expand mtu attempts to perform before setting final MTU.
        /// </summary>
        /// <remarks>
        /// Default value = 5.
        /// </remarks>
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

        /// <summary>
        /// Enables receiving a specified type of message.
        /// </summary>
        /// <param name="type"> The type to enable. </param>
        public void EnableMessageType(NetIncomingMessageType type)
        {
            disabledTypes &= (~type);
        }

        /// <summary>
        /// Disables receiving a specified type of message.
        /// </summary>
        /// <param name="type"> The type to disable. </param>
        public void DisableMessageType(NetIncomingMessageType type)
        {
            disabledTypes |= type;
        }

        /// <summary>
        /// Enables or disables a specified message type.
        /// </summary>
        /// <param name="type"> The type to change. </param>
        /// <param name="enabled"> Whether the type should be enabled. </param>
        public void SetMessageType(NetIncomingMessageType type, bool enabled)
        {
            if (enabled) EnableMessageType(type);
            else DisableMessageType(type);
        }

        /// <summary>
        /// Checks if a specified message type is enabled.
        /// </summary>
        /// <param name="type"> The type to check. </param>
        /// <returns> <see langword="true"/> if the message type was enabled; otherwise, <see langword="false"/>. </returns>
        public bool IsMessageTypeEnabled(NetIncomingMessageType type)
        {
            return (disabledTypes & type) != type;
        }

        /// <summary>
        /// Creates an unlocked copy of this <see cref="NetPeerConfig"/>.
        /// </summary>
        /// <returns> A unlocked clone of this <see cref="NetPeerConfig"/>. </returns>
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
