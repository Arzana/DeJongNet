namespace DeJong.Networking.Core.Msg
{
    using Connections;
    using MsgBuffer;
    using System.Diagnostics;
    using System.Net;

    [DebuggerDisplay("{{Type={Type}, Size={LengthBits}}}")]
    public sealed class NetIncomingMsg : MsgBuffer
    {
        public NetIncomingMessageType Type { get; private set; }
        public NetDeliveryMethod DevilveryMethod { get { return Utils.GetDeliveryMethod(receivedMsgType); } }
        public int SequenceChannel { get { return (int)receivedMsgType - (int)DevilveryMethod; } }
        public IPEndPoint SenderEndPoint { get; private set; }
        public NetConnection SenderConnection { get; private set; }
        public double ReceiveTime { get; private set; }

        private bool isFragment;
        private NetMsgType receivedMsgType;

        internal NetIncomingMsg(NetIncomingMessageType tp)
        {
            Type = tp;
        }

        internal void Reset()
        {
            Type = NetIncomingMessageType.None;
            PositionBits = 0;
            receivedMsgType = NetMsgType.LibraryError;
            SenderConnection = null;
            LengthBits = 0;
            isFragment = false;
        }

        public override string ToString()
        {
            return $"[{nameof(NetIncomingMsg)} #{SequenceChannel} {LengthBytes} bytes]";
        }
    }
}