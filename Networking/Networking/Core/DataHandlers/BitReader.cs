namespace DeJong.Networking.Core.DataHandlers
{
    using System;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class BitReader
    {
        // Check if length in withing range
        // Check if the offset if byte alligned and we need the full byte (possible return)
        // Get the needed part of the first byte
        // Check if the next byte is needed (possible return)
        // Get the needed part of the next byte
        // Compute result byte
        public static byte ReadByte(byte[] source, int offset, int length)
        {
            CheckOverflow(nameof(ReadByte), length, 8);

            int byteIndex = offset >> 3;
            int start = offset - (byteIndex << 3);
            if (start == 0 && length == 8) return source[byteIndex];

            byte result = (byte)(source[byteIndex] >> start);
            int bitsInNextByte = length - (8 - start);
            if (bitsInNextByte < 1) return (byte)(result & (255 >> (8 - length)));

            byte nextByte = source[byteIndex + 1];
            nextByte &= (byte)(255 >> (8 - bitsInNextByte));
            return (byte)(result | (byte)(nextByte << (length - bitsInNextByte)));
        }

        public static ushort ReadUInt16(byte[] source, int offset, int length)
        {
            CheckOverflow(nameof(ReadUInt16), length, 16);
            return (ushort)ReadVariableBytes(source, offset, length, sizeof(ushort));
        }

        public static uint ReadUInt32(byte[] source, int offset, int length)
        {
            CheckOverflow(nameof(ReadUInt32), length, 32);
            return (uint)ReadVariableBytes(source, offset, length, sizeof(uint));
        }

        public static ulong ReadUInt64(byte[] source, int offset, int length)
        {
            CheckOverflow(nameof(ReadUInt64), length, 64);
            return ReadVariableBytes(source, offset, length, sizeof(ulong));
        }

        public static void ReadBytes(byte[] source, int srcOffset, int length, byte[] destination, int destOffset)
        {
            int byteIndex = srcOffset >> 3;
            int start = srcOffset - (byteIndex << 3);

            if (start == 0) Buffer.BlockCopy(source, srcOffset, destination, destOffset, length);
            else
            {
                int nextPartLen = 8 - start;
                int nextMask = 255 >> nextPartLen;

                for (int i = 0; i < length; i++)
                {
                    int first = source[byteIndex++] >> start;
                    int second = source[byteIndex] & nextMask;
                    destination[destOffset++] = (byte)(first | (second << nextPartLen));
                }
            }
        }

        private static ulong ReadVariableBytes(byte[] source, int offset, int length, int numBytes)
        {
            ulong result = 0;
            for (int i = 0; i < numBytes && length > 0; i++, length -= 8, offset += 8)
            {
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                if (length <= 8) result |= (ulong)(ReadByte(source, offset, length) << (i << 3));
                else result |= (ulong)(ReadByte(source, offset, 8) << (i << 3));
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            }
            return result;
        }

        private static void CheckOverflow(string func, int check, int max)
        {
            LoggedException.RaiseIf(check < 1 || check > max, nameof(BitReader), $"{func} can only read between 1 and {max} bits");
        }
    }
}