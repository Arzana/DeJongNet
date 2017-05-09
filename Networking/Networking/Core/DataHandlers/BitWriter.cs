using DeJong.Utilities.Core;
using System;

namespace DeJong.Networking.Core.DataHandlers
{
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public static class BitWriter
    {
        // Check if length is withinf range
        // Get the needed part of the value
        // Check if a second byte is needed
        // Calculate mask(s)
        // Set byte in destination
        public static void WriteByte(byte[] destination, byte value, int offset, int length)
        {
            CheckOverflow(nameof(WriteByte), length, 8);
            value = (byte)(value & (255 >> (8 - length)));

            int byteIndex = offset >> 3;
            int usedBits = offset & 7;
            int freeBits = 8 - usedBits;
            int bitsLeft = freeBits - length;

            if (bitsLeft >= 0)
            {
                int mask = (255 >> freeBits) | (255 << (8 - bitsLeft));
                destination[byteIndex] = (byte)((destination[byteIndex] & mask) | (value << usedBits));
            }
            else
            {
                destination[byteIndex] = (byte)((destination[byteIndex] & (255 >> freeBits)) | (value << usedBits));
                destination[++byteIndex] = (byte)((destination[byteIndex] & (255 - freeBits)) | (value >> freeBits));
            }
        }

        public static void WriteUInt16(byte[] destination, ushort value, int offset, int length)
        {
            CheckOverflow(nameof(WriteUInt16), length, 16);
            WriteVariableBytes(destination, value, offset, length, sizeof(ushort));
        }

        public static void WriteUInt32(byte[] destination, uint value, int offset, int length)
        {
            CheckOverflow(nameof(WriteUInt32), length, 32);
            WriteVariableBytes(destination, value, offset, length, sizeof(int));
        }

        public static void WriteUInt64(byte[] destination, ulong value, int offset, int length)
        {
            CheckOverflow(nameof(WriteUInt64), length, 64);
            WriteVariableBytes(destination, value, offset, length, sizeof(ulong));
        }

        public static void WriteBytes(byte[] destination, byte[] value, int valueOffset, int length, int destOffset)
        {
            int byteIndex = destOffset >> 3;
            int firstPartLen = destOffset & 7;

            if (firstPartLen == 0) Buffer.BlockCopy(value, valueOffset, destination, destOffset, length);
            else
            {
                int nextPartLen = 8 - firstPartLen;
                for (int i = 0; i < length; i++)
                {
                    byte src = value[valueOffset + i];

                    destination[byteIndex] &= (byte)(255 >> nextPartLen);
                    destination[byteIndex++] |= (byte)(src << firstPartLen);

                    destination[byteIndex] &= (byte)(255 << firstPartLen);
                    destination[byteIndex] |= (byte)(src >> nextPartLen);
                }
            }
        }

        private static void WriteVariableBytes(byte[] destination, ulong value, int offset, int length, int numBytes)
        {
            for (int i = 0; i < numBytes && length > 0; i++, length -= 8, offset += 8)
            {
                if (length <= 8) WriteByte(destination, (byte)(value >> (i << 3)), offset, length);
                else WriteByte(destination, (byte)(value >> (i << 3)), offset, 8);
            }
        }

        private static void CheckOverflow(string func, int check, int max)
        {
            LoggedException.RaiseIf(check < 1 || check > max, nameof(BitReader), $"{func} can only write between 1 and {max} bits");
        }
    }
}