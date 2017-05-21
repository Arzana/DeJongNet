namespace DeJong.Networking.Core
{
    using System;
    using Peers;
    using Messages;

    public sealed class StatusChangedEventArgs : EventArgs
    {
        public readonly ConnectionStatus NewStatus;
        public readonly IncommingMsg Hail;
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