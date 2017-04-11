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
        /// Gets a <see cref="bool"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The byte at the current position as a <see cref="bool"/>. </returns>
        public bool PeekBool()
        {
            RaiseOverflowExceptionIf(1);
            byte result = BitReader.ReadByte(data, 1, position);
            return result != 0;
        }

        /// <summary>
        /// Gets a <see cref="byte"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The byte at the current position. </returns>
        public byte PeekByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitReader.ReadByte(data, BITS_PER_BYTE, position);
            return result;
        }

        /// <summary>
        /// Gets a <see cref="sbyte"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The byte at the current position as a <see cref="sbyte"/>. </returns>
        public sbyte PeekSByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitReader.ReadByte(data, BITS_PER_BYTE, position);
            return (sbyte)result;
        }

        /// <summary>
        /// Gets a <see cref="short"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 2 bytes as a <see cref="short"/>. </returns>
        public short PeekShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitReader.ReadUInt16(data, BITS_PER_INT16, position);
            return (short)result;
        }

        /// <summary>
        /// Gets a <see cref="ushort"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 2 bytes as a <see cref="ushort"/>. </returns>
        public ushort PeekUShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitReader.ReadUInt16(data, BITS_PER_INT16, position);
            return result;
        }

        /// <summary>
        /// Gets a <see cref="int"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="int"/>. </returns>
        public int PeekInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitReader.ReadUInt32(data, BITS_PER_INT32, position);
            return (int)result;
        }

        /// <summary>
        /// Gets a <see cref="uint"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="uint"/>. </returns>
        public uint PeekUInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitReader.ReadUInt32(data, BITS_PER_INT32, position);
            return result;
        }

        /// <summary>
        /// Gets a <see cref="long"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="long"/>. </returns>
        public long PeekLong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitReader.ReadUInt64(data, BITS_PER_INT64, position);
            return (long)result;
        }

        /// <summary>
        /// Gets a <see cref="ulong"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="ulong"/>. </returns>
        public ulong PeekULong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitReader.ReadUInt64(data, BITS_PER_INT64, position);
            return result;
        }

        /// <summary>
        /// Gets a <see cref="float"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 4 bytes as a <see cref="float"/>. </returns>
        public float PeekFloat()
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

            return result;
        }

        /// <summary>
        /// Gets a <see cref="double"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> The current 8 bytes as a <see cref="double"/>. </returns>
        public double PeekDouble()
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

            return result;
        }

        /// <summary>
        /// Gets a <see cref="string"/> at the current position without increasing the position.
        /// </summary>
        /// <returns> A the current bytes as a UTF8 <see cref="string"/>. </returns>
        public string PeekString()
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

            position -= BITS_PER_INT16;
            return result;
        }

        /// <summary>
        /// Gets multiple <see cref="bool"/> at the current position without increasing the position.
        /// </summary>
        /// <param name="amount"> The amount of <see cref="bool"/> to read. </param>
        /// <returns> The current bytes as multiple <see cref="bool"/> in a <see cref="BitFlags"/> class. </returns>
        public BitFlags PeekFlags(int amount)
        {
            BitFlags result = new BitFlags(amount);

            for (int i = 0; i < amount; i++)
            {
                result[i] = PeekBool();
            }

            return result;
        }
    }
}
