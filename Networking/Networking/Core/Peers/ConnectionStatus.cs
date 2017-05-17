namespace DeJong.Networking.Core.Peers
{
    public enum ConnectionStatus : byte
    {
        /// <summary>
        /// No connection or attempt in place.
        /// </summary>
        None,
        /// <summary>
        /// Connect has been sent; waiting for response.
        /// </summary>
        InitiatedConnect,
        /// <summary>
        /// Connect was received, but response hasn't been sent yet.
        /// </summary>
        ReceivedInitiation,
        /// <summary>
        /// Connect was received and aproval released to the application, awaiting aprove or deny.
        /// </summary>
        RespondedAwaitingApproval,
        /// <summary>
        /// Connect was received and response has been sent; waiting for connection established.
        /// </summary>
        RespondedConnected,
        /// <summary>
        /// Connected.
        /// </summary>
        Connected,
        /// <summary>
        /// In the process of disconnecting.
        /// </summary>
        Disconnecting,
        /// <summary>
        /// Disconnected.
        /// </summary>
        Disconnected
    }
}