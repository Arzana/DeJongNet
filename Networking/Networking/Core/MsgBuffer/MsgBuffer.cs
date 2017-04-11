namespace DeJong.Networking.Core.MsgBuffer
{
    using System;
    using System.Diagnostics;

#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("Buffer length={PositionBytes}")]
    public partial class MsgBuffer
    {
        /// <summary>
        /// Gets ot sets the data of the message buffer.
        /// </summary>
        public byte[] Data { get { return data; } set { data = value; } }

        /// <summary>
        /// Gets or sets the read position in the message buffer in bits.
        /// </summary>
        public long PositionBits { get { return position; } set { position = (int)value; } }

        /// <summary>
        /// Gets the read position in the message buffer in bytes.
        /// </summary>
        public int PositionBytes { get { return position / 8; } }

        /// <summary>
        /// Gets or sets the length of the used portion of the buffer in bits.
        /// </summary>
        public int LengthBits { get { return length; } set { EnsureBufferSize(length = value); } }

        /// <summary>
        /// Gets or sets the lenght of the used portion of the buffer in bytes.
        /// </summary>
        public int LengthBytes { get { return (length + 7) >> 3; } set { EnsureBufferSize(length = value << 3); } }

        private bool BitsAlligned { get { return (position & 7) == 0; } }

        private byte[] data;
        private int position;
        private int length;

        internal MsgBuffer() { }

        internal void EnsureBufferSize(int numBits)
        {
            int byteLen = (numBits + 7) >> 3;
            if (data == null) data = new byte[byteLen];
            else if (Data.Length < byteLen) Array.Resize(ref data, byteLen);
        }
    }
}