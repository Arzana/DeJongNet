namespace DeJong.Networking.Core.NetPeer
{
    /// <summary>
    /// Defines a status for a net peer instance.
    /// </summary>
    public enum NetPeerStatus : byte
    {
        /// <summary>
        /// The net peer is not runing; socket is not bound.
        /// </summary>
        NotRunning,
        /// <summary>
        /// The net peer is in the process of starting up.
        /// </summary>
        Starting,
        /// <summary>
        /// The net peer is bund to a socket and listening for packets.
        /// </summary>
        Running,
        /// <summary>
        /// Shutdown has been equested and will be executed shortly.
        /// </summary>
        ShutdownRequested
    }
}