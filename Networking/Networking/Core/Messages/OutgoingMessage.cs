namespace DeJong.Networking.Core.Messages
{
    using Utilities.Core;

    public sealed class OutgoingMsg : MsgBuffer
    {
        private MsgType type;
        private bool isSend;

        private int recyclingCount;
        private FragmentHeader fragHeader;

        internal LibHeader GenerateHeader(int sequenceNumber)
        {
            return new LibHeader(type, fragHeader.Group, sequenceNumber, LengthBytes);
        }

        internal void Reset()
        {
            LoggedException.RaiseIf(recyclingCount != 0, nameof(OutgoingMsg), $"Reset called on non garbage message");

            type = MsgType.LibraryError;
            LengthBits = 0;
            isSend = false;
            fragHeader.Reset();
        }

        public override string ToString()
        {
            return $"[{nameof(OutgoingMsg)}{(isSend ? $" {type}" : string.Empty)} {LengthBytes} bytes]";
        }
    }
}