namespace DeJong.Networking.Core.Messages
{
    using System;
    using System.Diagnostics;
    using Utilities.Core;

    [DebuggerDisplay("[{ToString()}]")]
    public sealed class IncommingMsg : MsgBuffer
    {
        internal LibHeader libHeader { get; private set; }
        internal FragmentHeader fragHeader { get; set; }

        internal IncommingMsg(byte[] buffer)
            : base(buffer)
        {
            libHeader = new LibHeader(this);
            if (libHeader.Fragment) fragHeader = new FragmentHeader(this);
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