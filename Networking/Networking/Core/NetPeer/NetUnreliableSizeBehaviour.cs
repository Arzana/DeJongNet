namespace DeJong.Networking.Core.NetPeer
{
    /// <summary>
    /// Defines the behaviour of unreliable sends above MTU.
    /// </summary>
    public enum NetUnreliableSizeBehaviour : byte
    {
        /// <summary>
        /// Sending an unreliable message will ignore MTU and send everything in a single packet.
        /// </summary>
        IgnoreMTU,
        /// <summary>
        /// Use normal fragmentation for unreliable messages.
        /// </summary>
        NormalFragmentation,
        /// <summary>
        /// Drops unreliable messages above MTU.
        /// </summary>
        DropAboveMTU
    }
}