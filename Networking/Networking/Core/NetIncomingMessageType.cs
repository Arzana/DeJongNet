namespace Mentula.Networking.Core
{
    /// <summary>
    /// The type of a incomming message.
    /// </summary>
    public enum NetIncomingMessageType : byte
    {
        /// <summary>
        /// Status for a connection changed.
        /// </summary>
        StatusChanged = 0,
        /// <summary>
        /// Data send using SendUnconnectedMessage.
        /// </summary>
        UnconnectedData = 1,
        /// <summary>
        /// Connection approval is needed.
        /// </summary>
        ConnectionApproval = 2,
        /// <summary>
        /// Application data.
        /// </summary>
        Data = 4,
        /// <summary>
        /// Receipt of delivery.
        /// </summary>
        Receipt = 8,
        /// <summary>
        /// Discovery request for a response.
        /// </summary>
        DiscoveryRequest = 16,
        /// <summary>
        /// Discovery response for a request.
        /// </summary>
        DiscoveryResponse = 32,
        /// <summary>
        /// NAT introduction was successfull.
        /// </summary>
        NatIntroductionSuccess = 64,
        /// <summary>
        /// A roundtrip was measured and NetConnection.AverageRoundtripTime was updated.
        /// </summary>
        ConnectionLatencyUpdate = 128
    }
}