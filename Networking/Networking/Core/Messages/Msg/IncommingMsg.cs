namespace DeJong.Networking.Core.Messages
{
    /// <summary>
    /// Defines an incomming networking message.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class IncommingMsg : ReadableBuffer
    {
        internal LibHeader Header { get; set; }

        internal IncommingMsg(byte[] data)
            : base(data)
        { }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(IncommingMsg)} {(Header.PacketSize + 7) >> 3} bytes";
        }
    }
}