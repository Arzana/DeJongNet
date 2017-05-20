namespace DeJong.Networking.Core.Messages
{
    using System.Diagnostics;

    [DebuggerDisplay("[{ToString()}]")]
    public sealed class IncommingMsg : ReadableBuffer
    {
        internal LibHeader Header { get; set; }

        internal IncommingMsg()
        { }

        internal IncommingMsg(byte[] data)
            : base(data)
        {
            Header = new LibHeader(this);
        }

        public override string ToString()
        {
            return $"{nameof(IncommingMsg)} {LengthBytes} bytes";
        }
    }
}