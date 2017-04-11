namespace DeJong.Networking.Core.BinaryData
{
    using System;
    using static BitUtils;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public static class BitReader
    {
        public static byte ReadByte(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_BYTE, $"{nameof(ReadByte)} can only read between 1 and 8 bits!");

            int byteIndex = bitIndex >> 3;
            int start = bitIndex - (byteIndex << 3);

            if (start == 0 && numBits == BITS_PER_BYTE) return buffer[byteIndex];

            byte result = (byte)(buffer[byteIndex] >> start);
            int numBitsInNextByte = numBits - (BITS_PER_BYTE - start);
            if (numBitsInNextByte < 1) return (byte)(result & (byte.MaxValue >> (BITS_PER_BYTE - numBits)));

            byte nextByte = buffer[byteIndex + 1];
            nextByte &= (byte)(byte.MaxValue >> (BITS_PER_BYTE - numBitsInNextByte));
            return (byte)(result | (byte)(nextByte << (numBits - numBitsInNextByte)));
        }

        public static void ReadBytes(byte[] buffer, int numBytes, int bitIndex, byte[] destination, int destinationByteIndex)
        {
            int byteIndex = bitIndex >> 3;
            int start = bitIndex - (byteIndex << 3);

            if (start == 0) Buffer.BlockCopy(buffer, byteIndex, destination, destinationByteIndex, numBytes);
            else
            {
                int nextPartLen = BITS_PER_BYTE - start;
                int nextMask = byte.MaxValue >> nextPartLen;

                for (int i = 0; i < numBytes; i++)
                {
                    int first = buffer[byteIndex++] >> start;
                    int second = buffer[byteIndex] & nextMask;
                    destination[destinationByteIndex++] = (byte)(first | (second << nextPartLen));
                }
            }
        }

        public static ushort ReadUInt16(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT16, $"{nameof(ReadUInt16)} can only read between 1 and 16 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(ushort); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return (ushort)result;
        }

        public static uint ReadUInt32(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT32, $"{nameof(ReadUInt32)} can only read between 1 and 32 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(uint); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return (uint)result;
        }

        public static ulong ReadUInt64(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT64, $"{nameof(ReadUInt64)} can only read between 1 and 64 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(ulong); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return result;
        }

        private static void InternalReadByte(byte[] buffer, int byteDepth, ref int numBits, ref int bitIndex, ref ulong result)
        {
            if (numBits <= 0) return;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            if (numBits <= BITS_PER_BYTE) result |= (ulong)(ReadByte(buffer, numBits, bitIndex) << (byteDepth << 3));
            else result |= (ulong)(ReadByte(buffer, BITS_PER_BYTE, bitIndex) << (byteDepth << 3));
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            numBits -= BITS_PER_BYTE;
            bitIndex += BITS_PER_BYTE;
        }
    }
}
