namespace DeJong.Networking.Core.Messages
{
    using Utilities.Core;

    public sealed class OutgoingMsg : MsgBuffer
    {
        internal bool IsSend { get; set; }

        private MsgType type;
        private int recyclingCount;
        private FragmentHeader fragHeader;

        internal OutgoingMsg(MsgType type)
        {
            this.type = type;
        }

        internal LibHeader GenerateHeader(int sequenceNumber)
        {
            return new LibHeader(type, fragHeader.Group, sequenceNumber, LengthBytes);
        }

        internal void Reset()
        {
            LoggedException.RaiseIf(recyclingCount != 0, nameof(OutgoingMsg), $"Reset called on non garbage message");

            type = MsgType.LibraryError;
            LengthBits = 0;
            IsSend = false;
            fragHeader.Reset();
        }

        public override string ToString()
        {
            return $"[{nameof(OutgoingMsg)}{(IsSend ? $" {type}" : string.Empty)} {LengthBytes} bytes]";
        }
    }
}