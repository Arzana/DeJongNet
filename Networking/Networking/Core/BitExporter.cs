namespace Mentula.Networking.Core
{
    using System;
    using static BitUtils;

    /// <summary>
    /// Defines functions for reading and writing unalligned bytes and basic datatypes.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public static class BitExporter
    {
        /// <summary>
        /// Reads between 1 and 8 bits from a buffer as a <see cref="byte"/>.
        /// </summary>
        /// <param name="buffer"> The buffer to read from. </param>
        /// <param name="numBits"> The amount of bits to read. </param>
        /// <param name="bitIndex"> A zero based offset from where to start reading. </param>
        /// <returns> The specified amount of bits as a <see cref="byte"/>. </returns>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
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

        /// <summary>
        /// Reads a variable amount of bytes from a buffer.
        /// </summary>
        /// <param name="buffer"> The buffer to read from. </param>
        /// <param name="numBytes"> The amount of bytes to read. </param>
        /// <param name="bitIndex"> A zero based offset from where to start reading. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="destinationByteIndex"> A zero based offset from where to start writing. </param>
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

        /// <summary>
        /// Writes between 1 and 8 bits to a specified buffer.
        /// </summary>
        /// <param name="value"> A container for the value. </param>
        /// <param name="numBits"> The amount of bits to write. </param>
        /// <param name="destination"> The destination buffer. </param>
        /// <param name="destinationBitIndex"> A zero based offset from where to start writing. </param>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
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

        /// <summary>
        /// Writes a variable amount of bytes to a specified buffer.
        /// </summary>
        /// <param name="value"> A buffer from where to read the values. </param>
        /// <param name="valueByteIndex"> A zero based index from where to start reading. </param>
        /// <param name="numBytes"> The amount of bytes to read. </param>
        /// <param name="destination"> The destination buffer. </param>
        /// <param name="destinationBitIndex"> A zero based offset from where to start writing. </param>
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

        /// <summary>
        /// Reads between 1 and 16 bits from a specified buffer as an <see cref="ushort"/>.
        /// </summary>
        /// <param name="buffer"> The buffer to red from. </param>
        /// <param name="numBits"> The amount of bits to read. </param>
        /// <param name="bitIndex"> A zero based offset from where to start reading. </param>
        /// <returns> The specified amount of bits as a <see cref="ushort"/>. </returns>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static ushort ReadUInt16(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT16, $"{nameof(ReadUInt16)} can only read between 1 and 16 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(ushort); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return (ushort)result;
        }

        /// <summary>
        /// Reads between 1 and 32 bits from a specified buffer as an <see cref="uint"/>.
        /// </summary>
        /// <param name="buffer"> The buffer to red from. </param>
        /// <param name="numBits"> The amount of bits to read. </param>
        /// <param name="bitIndex"> A zero based offset from where to start reading. </param>
        /// <returns> The specified amount of bits as a <see cref="uint"/>. </returns>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static uint ReadUInt32(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT32, $"{nameof(ReadUInt32)} can only read between 1 and 32 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(uint); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return (uint)result;
        }

        /// <summary>
        /// Reads between 1 and 64 bits from a specified buffer as an <see cref="ulong"/>.
        /// </summary>
        /// <param name="buffer"> The buffer to red from. </param>
        /// <param name="numBits"> The amount of bits to read. </param>
        /// <param name="bitIndex"> A zero based offset from where to start reading. </param>
        /// <returns> The specified amount of bits as a <see cref="ulong"/>. </returns>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static ulong ReadUInt64(byte[] buffer, int numBits, int bitIndex)
        {
            NetException.RaiseIf(numBits <= 0 || numBits > BITS_PER_INT64, $"{nameof(ReadUInt64)} can only read between 1 and 64 bits!");

            ulong result = 0;
            for (int i = 0; i < sizeof(ulong); i++) InternalReadByte(buffer, i, ref numBits, ref bitIndex, ref result);
            return result;
        }

        /// <summary>
        /// Writes between 1 and 16 bits to a specified buffer.
        /// </summary>
        /// <param name="value"> A container for the value. </param>
        /// <param name="numBits"> The amount of bits to write. </param>
        /// <param name="destination"> The destination buffer. </param>
        /// <param name="destinatioBitIndex"> A zero based offset from where to start writing. </param>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static void WriteUInt16(ushort value, int numBits, byte[] destination, int destinatioBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT16, $"{nameof(WriteUInt16)} can only write between 1 and 16 bits!");

            for (int i = 0; i < sizeof(ushort); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinatioBitIndex);
        }

        /// <summary>
        /// Writes between 1 and 32 bits to a specified buffer.
        /// </summary>
        /// <param name="value"> A container for the value. </param>
        /// <param name="numBits"> The amount of bits to write. </param>
        /// <param name="destination"> The destination buffer. </param>
        /// <param name="destinatioBitIndex"> A zero based offset from where to start writing. </param>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static void WriteUInt32(uint value, int numBits, byte[] destination, int destinationBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT32, $"{nameof(WriteUInt32)} can only write between 1 and 32 bits!");

            for (int i = 0; i < sizeof(uint); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinationBitIndex);
        }

        /// <summary>
        /// Writes between 1 and 64 bits to a specified buffer.
        /// </summary>
        /// <param name="value"> A container for the value. </param>
        /// <param name="numBits"> The amount of bits to write. </param>
        /// <param name="destination"> The destination buffer. </param>
        /// <param name="destinatioBitIndex"> A zero based offset from where to start writing. </param>
        /// <exception cref="NetException"> <paramref name="numBits"/> was set out of bounds. </exception>
        public static void WriteUInt64(ulong value, int numBits, byte[] destination, int destinationBitIndex)
        {
            if (numBits == 0) return;
            NetException.RaiseIf(numBits < 0 || numBits > BITS_PER_INT64, $"{nameof(WriteUInt64)} can only write between 1 and 64 bits!");

            for (int i = 0; i < sizeof(ulong); i++) InternalWriteByte(value, destination, i, ref numBits, ref destinationBitIndex);
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
