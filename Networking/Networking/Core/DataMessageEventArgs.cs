namespace DeJong.Networking.Core
{
    using Messages;
    using System;

    /// <summary>
    /// Represent a class that contains the event data for normal data messages between peers.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DataMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the method of delivery for this message.
        /// </summary>
        public readonly DeliveryMethod Method;
        /// <summary>
        /// Gets the channel over wich this message was send.
        /// </summary>
        public readonly int Channel;
        /// <summary>
        /// Get the actual data message.
        /// </summary>
        public readonly IncommingMsg Message;

        internal DataMessageEventArgs(IncommingMsg msg)
        {
            Method = (DeliveryMethod)msg.Header.Type;
            Channel = msg.Header.Channel;
            Message = msg;
        }
    }
}