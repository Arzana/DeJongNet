using DeJong.Utilities.Core;
using System;
using System.Runtime.InteropServices;

namespace DeJong.Networking.Core.Messages
{
    /// <summary>
    /// Defines a base class for reading and writing network messages.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract partial class MsgBuffer
    {
        /// <summary>
        /// Gets or sets the read position of the message buffer in bits.
        /// </summary>
        public int PositionBits { get { return position; } set { position = value; } }
        /// <summary>
        /// Gets or sets the read position of the message buffer in bytes.
        /// </summary>
        public int PositionBytes { get { return position >> 3; } set { position = value << 3; } }
        /// <summary>
        /// Gets or sets the length of the message buffer in bits.
        /// </summary>
        public int LengthBits { get { return length; } set { EnsureBufferSize(length = value); } }
        /// <summary>
        /// Gets ot sets the length of the message buffer in bytes.
        /// </summary>
        public int LengthBytes { get { return (length + 7) >> 3; } set { EnsureBufferSize(length = value << 3); } }

        private bool BitAlligned { get { return (position % 8) == 0; } }

        private byte[] data;
        private int position;
        private int length;

        internal MsgBuffer(int initialSize)
        {
            EnsureBufferSize(initialSize);
        }

        internal void CopyData(byte[] destination, int offset)
        {
            Array.Copy(data, 0, destination, offset, data.Length);
        }

        private void EnsureBufferSize(int numBits)
        {
            int byteLen = (numBits + 7) >> 3;
            if (data == null) data = new byte[byteLen];
            else if (data.Length < byteLen) Array.Resize(ref data, byteLen);
        }

        private void CheckOverflow(int bitsNeeded)
        {
            LoggedException.RaiseIf((data.Length << 3) - position < bitsNeeded, nameof(MsgBuffer), "Cannot read past buffer size");
        }

        [StructLayout(LayoutKind.Explicit, Size = 4)]
        private struct IntSingleUnion
        {
            [FieldOffset(0)]
            public uint IntValue;

            [FieldOffset(0)]
            public float SingleValue;

            public IntSingleUnion(uint value)
            {
                SingleValue = 0;
                IntValue = value;
            }

            public IntSingleUnion(float value)
            {
                IntValue = 0;
                SingleValue = value;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct IntDoubleUnion
        {
            [FieldOffset(0)]
            public ulong IntValue;

            [FieldOffset(0)]
            public double DoubleValue;

            public IntDoubleUnion(ulong value)
            {
                DoubleValue = 0;
                IntValue = value;
            }

            public IntDoubleUnion(double value)
            {
                IntValue = 0;
                DoubleValue = value;
            }
        }
    }
}