namespace DeJong.Networking.Core.Messages
{
    using System.Diagnostics;

    [DebuggerDisplay("[{ToString()}]")]
    public sealed class IncommingMsg : ReadableBuffer
    {
        public IncommingMsgType Type { get; private set; }

        internal LibHeader Header { get; set; }

        public override string ToString()
        {
            return $"{nameof(IncommingMsg)} {LengthBytes} bytes";
        }
    }
}