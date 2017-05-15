namespace DeJong.Networking.Core.Messages
{
    public sealed class OutgoingMsg : WriteableBuffer
    {
        internal bool IsSend { get; set; }

        private MsgType type;

        internal OutgoingMsg(MsgType type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return $"[{nameof(OutgoingMsg)}{(IsSend ? $" {type}" : string.Empty)} {LengthBytes} bytes]";
        }
    }
}