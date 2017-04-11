namespace DeJong.Networking.Core.MsgBuffer
{
    using BinaryData;
    using DataTypes;
    using System;
    using System.Text;
    using static BinaryData.BitUtils;

    public partial class MsgBuffer
    {
        /// <summary>
        /// Gets a <see cref="bool"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The byte at the current position as a <see cref="bool"/>. </returns>
        public bool ReadBool()
        {
            RaiseOverflowExceptionIf(1);
            byte result = BitReader.ReadByte(data, 1, position);
            position += 1;
            return result != 0;
        }

        /// <summary>
        /// Gets a <see cref="byte"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The byte at the current position. </returns>
        public byte ReadByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitReader.ReadByte(data, BITS_PER_BYTE, position);
            position += BITS_PER_BYTE;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="sbyte"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The byte at the current position as a <see cref="sbyte"/>. </returns>
        public sbyte ReadSByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitReader.ReadByte(data, BITS_PER_BYTE, position);
            position += BITS_PER_BYTE;
            return (sbyte)result;
        }

        /// <summary>
        /// Gets a <see cref="short"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 2 bytes as a <see cref="short"/>. </returns>
        public short ReadShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitReader.ReadUInt16(data, BITS_PER_INT16, position);
            position += BITS_PER_INT16;
            return (short)result;
        }

        /// <summary>
        /// Gets a <see cref="ushort"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 2 bytes as a <see cref="ushort"/>. </returns>
        public ushort ReadUShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitReader.ReadUInt16(data, BITS_PER_INT16, position);
            position += BITS_PER_INT16;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="int"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="int"/>. </returns>
        public int ReadInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitReader.ReadUInt32(data, BITS_PER_INT32, position);
            position += BITS_PER_INT32;
            return (int)result;
        }

        /// <summary>
        /// Gets a <see cref="uint"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="uint"/>. </returns>
        public uint ReadUInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitReader.ReadUInt32(data, BITS_PER_INT32, position);
            position += BITS_PER_INT32;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="long"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="long"/>. </returns>
        public long ReadLong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitReader.ReadUInt64(data, BITS_PER_INT64, position);
            position += BITS_PER_INT64;
            return (long)result;
        }

        /// <summary>
        /// Gets a <see cref="ulong"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="ulong"/>. </returns>
        public ulong ReadULong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitReader.ReadUInt64(data, BITS_PER_INT64, position);
            position += BITS_PER_INT64;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="float"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="float"/>. </returns>
        public float ReadFloat()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            float result;

            if (BitsAlligned) result = BitConverter.ToSingle(data, position >> 3);
            else
            {
                byte[] bytes = new byte[sizeof(float)];
                BitReader.ReadBytes(data, sizeof(float), position, bytes, 0);
                result = BitConverter.ToSingle(bytes, 0);
            }

            position += BITS_PER_INT32;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="double"/> at the current position and increases the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="double"/>. </returns>
        public double ReadDouble()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            double result;

            if (BitsAlligned) result = BitConverter.ToDouble(data, position >> 3);
            else
            {
                byte[] bytes = new byte[sizeof(double)];
                BitReader.ReadBytes(data, sizeof(double), position, bytes, 0);
                result = BitConverter.ToDouble(bytes, 0);
            }

            position += BITS_PER_INT64;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="string"/> at the current position and increases the position.
        /// </summary>
        /// <returns> A the current bytes as a UTF8 <see cref="string"/>. </returns>
        public string ReadString()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            int length = ReadShort();
            if (length <= 0) return string.Empty;

            RaiseOverflowExceptionIf(length);

            string result;
            if (BitsAlligned) result = Encoding.UTF8.GetString(data, position >> 3, length);
            else
            {
                byte[] bytes = new byte[length];
                BitReader.ReadBytes(data, length, position, bytes, 0);
                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            position += length << 3;
            return result;
        }

        /// <summary>
        /// Gets multiple <see cref="bool"/> at the current position and increases the position.
        /// </summary>
        /// <param name="amount"> The amount of <see cref="bool"/> to read. </param>
        /// <returns> The current bytes as multiple <see cref="bool"/> in a <see cref="BitFlags"/> class. </returns>
        public BitFlags ReadFlags(int amount)
        {
            BitFlags result = new BitFlags(amount);

            for (int i = 0; i < amount; i++)
            {
                result[i] = ReadBool();
            }

            return result;
        }

        /// <summary>
        /// Skips all bits until the position is byte alligned.
        /// </summary>
        public void ReadPadBits()
        {
            position = ((position + 7) >> 3) << 3;
        }

        /// <summary>
        /// Skips a speicified amount of bits.
        /// </summary>
        /// <param name="amount"> The amount of bits to skip. </param>
        public void ReadPadBits(int amount)
        {
            position += amount;
        }

        private void RaiseOverflowExceptionIf(int bitsNeeded)
        {
            NetException.RaiseIf((data.Length << 3) - position < bitsNeeded, "Cannot read past the buffer size!");
        }
    }
}