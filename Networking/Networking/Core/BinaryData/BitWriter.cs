namespace DeJong.Networking.Core.BinaryData
{
    using System;
    using static BitUtils;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class BitWriter
    {
        public static void WriteByte(byte value, int numBits, byte[] destination, int destinationBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > 8, $"{nameof(WriteByte)} can only write between 1 and 8 bits!");

            value = (byte)(value & (byte.MaxValue >> (BITS_PER_BYTE - numBits)));

            int byteIndex = destinationBitIndex >> 3;
            int bitsUsed = destinationBitIndex & 7;
            int bitsFree = BITS_PER_BYTE - bitsUsed;
            int bitsLeft = bitsFree - numBits;

            if (bitsLeft >= 0)
            {
                int mask = (byte.MaxValue >> bitsFree) | (byte.MaxValue << (BITS_PER_BYTE - bitsLeft));
                destination[byteIndex] = (byte)((destination[byteIndex] & mask) | (value << bitsUsed));
            }
            else
            {
                destination[byteIndex] = (byte)((destination[byteIndex] & (byte.MaxValue >> bitsFree)) | (value << bitsUsed));
                destination[++byteIndex] = (byte)((destination[byteIndex] & (byte.MaxValue - bitsFree)) | (value >> bitsFree));
            }
        }

        public static void WriteBytes(byte[] value, int valueByteIndex, int numBytes, byte[] destination, int destinationBitIndex)
        {
            int byteIndex = destinationBitIndex >> 3;
            int firstPartLen = destinationBitIndex & 7;

            if (firstPartLen == 0) Buffer.BlockCopy(value, valueByteIndex, destination, byteIndex, numBytes);
            else
            {
                int lastPartLen = BITS_PER_BYTE - firstPartLen;
                for (int i = 0; i < numBytes; i++)
                {
                    byte src = value[valueByteIndex + i];

                    destination[byteIndex] &= (byte)(byte.MaxValue >> lastPartLen);
                    destination[byteIndex++] |= (byte)(src << firstPartLen);

                    destination[byteIndex] &= (byte)(byte.MaxValue << firstPartLen);
                    destination[byteIndex] |= (byte)(src >> lastPartLen);
                }
            }
        }

        public static void WriteUInt16(ushort value, int numBits, byte[] destination, int destinatioBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT16, $"{nameof(WriteUInt16)} can only write between 1 and 16 bits!");

            for (int i = 0; i < sizeof(ushort); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinatioBitIndex);
        }

        public static void WriteUInt32(uint value, int numBits, byte[] destination, int destinationBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT32, $"{nameof(WriteUInt32)} can only write between 1 and 32 bits!");

            for (int i = 0; i < sizeof(uint); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinationBitIndex);
        }

        public static void WriteUInt64(ulong value, int numBits, byte[] destination, int destinationBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT64, $"{nameof(WriteUInt64)} can only write between 1 and 64 bits!");

            for (int i = 0; i < sizeof(ulong); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinationBitIndex);
        }

        private static void InternalWriteByte(ulong value, byte[] destination, int byteDepth, ref int numBits, ref int bitIndex)
        {
            if (numBits <= 0) return;

            if (numBits <= BITS_PER_BYTE) WriteByte((byte)(value >> (byteDepth << 3)), numBits, destination, bitIndex);
            else WriteByte((byte)(value >> (byteDepth << 3)), BITS_PER_BYTE, destination, bitIndex);

            numBits -= BITS_PER_BYTE;
            bitIndex += BITS_PER_BYTE;
        }
    }
}
