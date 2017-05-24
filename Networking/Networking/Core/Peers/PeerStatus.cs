namespace DeJong.Networking.Core.Peers
{
    /// <summary>
    /// Represent the states of a networking peer.
    /// </summary>
    public enum PeerStatus : byte
    {
        /// <summary>
        /// The peer has not yet started or has been shut down.
        /// </summary>
        NotRunning,
        /// <summary>
        /// The peer is initializing its networking components.
        /// </summary>
        Starting,
        /// <summary>
        /// The peer is operational.
        /// </summary>
        Running,
        /// <summary>
        /// A shutdown has been requested the peer is in the process of closing.
        /// </summary>
        ShutdownRequested
    }
}