namespace DeJong.Networking.Core.Messages
{
    /// <summary>
    /// Defines an outgoing networking message.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class OutgoingMsg : WriteableBuffer
    {
        internal bool IsSend { get; set; }
        internal int SequenceNumber { get; set; }
        internal int channel { get; private set; }

        private MsgType type;

        internal OutgoingMsg(int channel, MsgType type, byte[] buffer)
            : base(buffer)
        {
            this.channel = channel;
            this.type = type;
        }

        internal LibHeader GenerateHeader(int mtu)
        {
            return new LibHeader(type, channel, LengthBytes >= mtu, SequenceNumber, LengthBits);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{nameof(OutgoingMsg)}{(IsSend ? $" {type}" : string.Empty)} {LengthBytes} bytes]";
        }
    }
}