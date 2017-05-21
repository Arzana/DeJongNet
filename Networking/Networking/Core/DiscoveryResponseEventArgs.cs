namespace DeJong.Networking.Core
{
    using Messages;
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class SimpleMessageEventArgs : EventArgs
    {
        public bool HasAdditionalMessage { get { return Message.Header.PacketSize > 0; } }
        public readonly IncommingMsg Message;

        internal SimpleMessageEventArgs(IncommingMsg msg)
        {
            Message = msg;
        }
    }
}