namespace DeJong.Networking.Core
{
    using Messages;
    using System;

    /// <summary>
    /// Represent a class that contains the event data for discovery and connect events.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class SimpleMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates whether a hail message is pressent.
        /// </summary>
        public bool HasAdditionalMessage { get { return Message?.Header.PacketSize > 0; } }
        /// <summary>
        /// A hail messages.
        /// </summary>
        public readonly IncommingMsg Message;

        internal SimpleMessageEventArgs(IncommingMsg msg)
        {
            Message = msg;
        }
    }
}