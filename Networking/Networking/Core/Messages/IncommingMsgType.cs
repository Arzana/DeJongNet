namespace DeJong.Networking.Core.Messages
{
    public enum IncommingMsgType : byte
    {
        /// <summary>
        /// Error; this value should never apprear.
        /// </summary>
        /// <remarks> No data. </remarks>
        Error = 0,
        /// <summary>
        /// Status for the connection changed.
        /// </summary>
        /// <remarks> String data. </remarks>
        StatusChanged = 1,
        /// <summary>
        /// Data send to or from a unconnected host.
        /// </summary>
        /// <remarks> User data. </remarks>
        UnconnectedData = 2,
        /// <summary>
        /// Connection approval is needed.
        /// </summary>
        /// <remarks> User data. </remarks>
        ConnectionApproval = 4,
        /// <summary>
        /// Application data.
        /// </summary>
        /// <remarks> User data. </remarks>
        Data = 8,
        /// <summary>
        /// Receipt of delivery.
        /// </summary>
        /// <remarks> Data. </remarks>
        Receipt = 16,
        /// <summary>
        /// Discovery request for a response.
        /// </summary>
        /// <remarks> No data. </remarks>
        DiscoveryRequest = 32,
        /// <summary>
        /// Discovery response to a request.
        /// </summary>
        /// <remarks> Data. </remarks>
        DiscoveryResponse = 64,
    }
}