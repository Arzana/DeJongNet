namespace DeJong.Networking.Core.Messages
{
    /// <summary>
    /// Defines how the message should be handled.
    /// </summary>
    public enum DeliveryMethod : byte
    {
        /// <summary>
        /// Indicates an error.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Messages can be lost and are not ordered.
        /// </summary>
        Unreliable = 2,
        /// <summary>
        /// Messages can be lost and are dropped when late.
        /// </summary>
        Ordered = 3,
        /// <summary>
        /// Messages cannot be lost but are not ordered.
        /// </summary>
        Reliable = 4,
        /// <summary>
        /// Messages cannot be lost and are dropped when late.
        /// </summary>
        ReliableOrdered = 5,
    }
}