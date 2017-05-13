namespace DeJong.Networking.Core.Messages
{
    using System;
    using System.Diagnostics;
    using Utilities.Core;

    [DebuggerDisplay("[{ToString()}]")]
    public sealed class IncommingMsg : ReadableBuffer
    {
        public IncommingMsgType Type { get; private set; }

        internal LibHeader libHeader { get; private set; }
        internal FragmentHeader fragHeader { get; set; }

        internal IncommingMsg(byte[] buffer)
            : base(buffer)
        {
            libHeader = new LibHeader(this);
            if (libHeader.Fragment) fragHeader = new FragmentHeader(this);

            switch (libHeader.Type)
            {
                case MsgType.Unconnected:
                    Type = IncommingMsgType.UnconnectedData;
                    break;
                case MsgType.Unreliable:
                case MsgType.UnreliableOrdered:
                case MsgType.Reliable:
                case MsgType.ReliableOrdered:
                    Type = IncommingMsgType.Data;
                    break;
                case MsgType.Connect:
                    Type = IncommingMsgType.ConnectionApproval;
                    break;
                case MsgType.ConnectionEstablished:
                case MsgType.Disconnect:
                    Type = IncommingMsgType.StatusChanged;
                    break;
                case MsgType.Acknowledge:
                    Type = IncommingMsgType.Receipt;
                    break;
                case MsgType.Discovery:
                    Type = IncommingMsgType.DiscoveryRequest;
                    break;
                case MsgType.DiscoveryResponse:
                    Type = IncommingMsgType.DiscoveryResponse;
                    break;
                default:
                    Type = IncommingMsgType.Error;
                    break;
            }
        }

        internal IncommingMsg(params IncommingMsg[] fragments)
        {
            CheckFragments(fragments);

            libHeader = fragments[0].libHeader;

            EnsureBufferSize(fragments[0].fragHeader.TotalBits);
            for (int i = 0; i < fragments.Length; i++)
            {
                IncommingMsg cur = fragments[i];
                Array.Copy(cur.data, 0, data, LengthBytes, cur.data.Length);
                LengthBytes += cur.data.Length;
            }
        }

        public override string ToString()
        {
            return $"{nameof(IncommingMsg)} {LengthBytes} bytes";
        }

        private static void CheckFragments(IncommingMsg[] fragments)
        {
            LoggedException.RaiseIf(fragments == null || fragments.Length < 1, nameof(IncommingMsg), "Cannot construct message from empty fragments");
        }
    }
}