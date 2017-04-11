namespace DeJong.Networking.Core.Msg
{
    /// <summary>
    /// The type of a incomming message.
    /// </summary>
    public enum NetIncomingMessageType : short
    {
        /// <summary>
        /// This value should never appear.
        /// </summary>
        None = 0,
        /// <summary>
        /// Status for a connection changed.
        /// </summary>
        StatusChanged = 1,
        /// <summary>
        /// Data send using SendUnconnectedMessage.
        /// </summary>
        UnconnectedData = 2,
        /// <summary>
        /// Connection approval is needed.
        /// </summary>
        ConnectionApproval = 4,
        /// <summary>
        /// Application data.
        /// </summary>
        Data = 8,
        /// <summary>
        /// Receipt of delivery.
        /// </summary>
        Receipt = 16,
        /// <summary>
        /// Discovery request for a response.
        /// </summary>
        DiscoveryRequest = 32,
        /// <summary>
        /// Discovery response for a request.
        /// </summary>
        DiscoveryResponse = 64,
        /// <summary>
        /// NAT introduction was successfull.
        /// </summary>
        NatIntroductionSuccess = 128,
        /// <summary>
        /// A roundtrip was measured and NetConnection.AverageRoundtripTime was updated.
        /// </summary>
        ConnectionLatencyUpdate = 256
    }
}