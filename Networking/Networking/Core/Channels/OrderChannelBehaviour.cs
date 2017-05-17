namespace DeJong.Networking.Core.Channels
{
    /// <summary>
    /// Defines how a ordered channel should handle late messages.
    /// </summary>
    public enum OrderChannelBehaviour : byte
    {
        /// <summary>
        /// When caught early, late messages should be ordered before later ones.
        /// </summary>
        Order,
        /// <summary>
        /// When caught early, late messages should be dropped.
        /// </summary>
        Drop
    }
}
