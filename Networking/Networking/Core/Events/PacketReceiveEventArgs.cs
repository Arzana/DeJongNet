namespace DeJong.Networking.Core
{
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class PacketReceiveEventArgs : EventArgs
    {
        public readonly int PacketSize;

        public PacketReceiveEventArgs(int size)
        {
            PacketSize = size;
        }
    }
}