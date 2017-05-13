namespace DeJong.Networking.Core.Messages
{
    using System.Collections.Generic;

    public sealed class OutgoingMsg : WriteableBuffer
    {
        internal bool IsSend { get; set; }

        private MsgType type;

        internal OutgoingMsg(MsgType type)
        {
            this.type = type;
        }

        internal LibHeader GenerateHeader(int sequenceNumber, int mtu)
        {
            return new LibHeader(type, NeedsFragmentation(mtu), sequenceNumber, LengthBytes);
        }

        internal Dictionary<FragmentHeader, OutgoingMsg> ToFragments(int group, int mtu)
        {
            Dictionary<FragmentHeader, OutgoingMsg> result = new Dictionary<FragmentHeader, OutgoingMsg>();
            int fragmentSize = FragmentHeader.GetChunkSize(group, LengthBytes, mtu);

            int bytesLeft = LengthBytes, curFragmentSize = fragmentSize;
            for (int i = 0; bytesLeft > 0; i++)
            {
                if (curFragmentSize > bytesLeft) curFragmentSize = bytesLeft;
                bytesLeft -= curFragmentSize;
                FragmentHeader header = new FragmentHeader(group, LengthBits, curFragmentSize, i);

                OutgoingMsg msg = new OutgoingMsg(type);
                CopyData(msg, i * fragmentSize, header.FragmentSize);

                result.Add(header, msg);
            }

            return result;
        }

        public override string ToString()
        {
            return $"[{nameof(OutgoingMsg)}{(IsSend ? $" {type}" : string.Empty)} {LengthBytes} bytes]";
        }

        private bool NeedsFragmentation(int mtu)
        {
            return LengthBytes >= mtu;
        }
    }
}