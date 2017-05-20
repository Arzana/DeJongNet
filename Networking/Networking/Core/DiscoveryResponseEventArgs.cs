namespace DeJong.Networking.Core
{
    using Messages;
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DiscoveryResponseEventArgs : EventArgs
    {
        public bool HasAdditionalMessage { get { return Message.Header.PacketSize > 0; } }
        public readonly IncommingMsg Message;

        internal DiscoveryResponseEventArgs(IncommingMsg msg)
        {
            Message = msg;
        }
    }
}