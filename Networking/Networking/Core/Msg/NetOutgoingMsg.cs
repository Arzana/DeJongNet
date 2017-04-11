namespace DeJong.Networking.Core.Msg
{
    using MsgBuffer;
    using System.Diagnostics;

    [DebuggerDisplay("Size={LengthBits}")]
    public sealed class NetOutgoingMsg : MsgBuffer
    {
        private NetMsgType type;
        private bool isSend;

        // Recycling count is:
        // + Incremented for each recipient on send.
        // + Incremented, when reliable, in SenderChannel.ExecuteSend().
        // - Decremented (both reliable and unreliable) in NetConnection.QueueSendMsg().
        // - Decremented, when reliable, in SenderChannel.DestoreMsg().
        // When it reaches 0 it can be recycled.
        private int recyclingCount;

        private int fragmentGroup;
        private int fragmentGroupTotalBits;
        private int fragmentChunkByteSize;
        private int fragmentChunkNumber;

        internal NetOutgoingMsg() { }

        internal void Reset()
        {
            type = NetMsgType.LibraryError;
            LengthBits = 0;
            isSend = false;
            NetException.RaiseIf(recyclingCount != 0);
            fragmentGroup = 0;
        }

        public override string ToString()
        {
            return isSend ? $"[{nameof(NetOutgoingMsg)} {type} {LengthBytes} bytes]" : $"[{nameof(NetOutgoingMsg)} {LengthBytes} bytes]";
        }
    }
}