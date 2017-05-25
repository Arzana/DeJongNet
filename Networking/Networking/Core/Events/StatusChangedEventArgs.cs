namespace DeJong.Networking.Core
{
    using System;
    using Peers;
    using Messages;

    /// <summary>
    /// Represent a class that contains the event data for a change in connection status.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class StatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new status of the connection.
        /// </summary>
        public readonly ConnectionStatus NewStatus;
        /// <summary>
        /// The connection hail message (only set if NewStatus is Connected).
        /// </summary>
        public readonly IncommingMsg Hail;
        /// <summary>
        /// The reason for disocnnecting (only set if NetStatus is Disconnected).
        /// </summary>
        public readonly string Reason;

        internal StatusChangedEventArgs(IncommingMsg hail)
        {
            NewStatus = ConnectionStatus.Connected;
            Hail = hail;
            Reason = string.Empty;
        }

        internal StatusChangedEventArgs(string reason)
        {
            NewStatus = ConnectionStatus.Disconnected;
            Reason = reason;
        }
    }
}