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
        /// Writes a <see cref="bool"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="bool"/> to write. </param>
        public void Write(bool value)
        {
            EnsureBufferSize(position + 1);
            BitWriter.WriteByte((byte)(value ? 1 : 0), 1, data, length);
            length += 1;
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="byte"/> to write. </param>
        public void Write(byte value)
        {
            EnsureBufferSize(position + BITS_PER_BYTE);
            BitWriter.WriteByte(value, BITS_PER_BYTE, data, length);
            length += BITS_PER_BYTE;
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="sbyte"/> to write. </param>
        public void Write(sbyte value)
        {
            EnsureBufferSize(position + BITS_PER_BYTE);
            BitWriter.WriteByte((byte)value, BITS_PER_BYTE, data, length);
            length += BITS_PER_BYTE;
        }

        /// <summary>
        /// Writes a <see cref="short"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="short"/> to write. </param>
        public void Write(short value)
        {
            EnsureBufferSize(position + BITS_PER_INT16);
            BitWriter.WriteUInt16((ushort)value, BITS_PER_INT16, data, length);
            length += BITS_PER_INT16;
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="ushort"/> to write. </param>
        public void Write(ushort value)
        {
            EnsureBufferSize(position + BITS_PER_INT16);
            BitWriter.WriteUInt16(value, BITS_PER_INT16, data, length);
            length += BITS_PER_INT16;
        }

        /// <summary>
        /// Writes a <see cref="int"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="int"/> to write. </param>
        public void Write(int value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            BitWriter.WriteUInt32((uint)value, BITS_PER_INT32, data, length);
            length += BITS_PER_INT32;
        }

        /// <summary>
        /// Writes a <see cref="uint"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="uint"/> to write. </param>
        public void Write(uint value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            BitWriter.WriteUInt32(value, BITS_PER_INT32, data, length);
            length += BITS_PER_INT32;
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="long"/> to write. </param>
        public void Write(long value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            BitWriter.WriteUInt64((ulong)value, BITS_PER_INT64, data, length);
            length += BITS_PER_INT64;
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="ulong"/> to write. </param>
        public void Write(ulong value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            BitWriter.WriteUInt64(value, BITS_PER_INT64, data, length);
            length += BITS_PER_INT64;
        }

        /// <summary>
        /// Writes a <see cref="float"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="float"/> to write. </param>
        public void Write(float value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            byte[] bytes = BitConverter.GetBytes(value);
            BitWriter.WriteBytes(bytes, 0, sizeof(float), data, length);
            length += BITS_PER_INT32;
        }

        /// <summary>
        /// Writes a <see cref="double"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="double"/> to write. </param>
        public void Write(double value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            byte[] bytes = BitConverter.GetBytes(value);
            BitWriter.WriteBytes(bytes, 0, sizeof(double), data, length);
            length += BITS_PER_INT64;
        }

        /// <summary>
        /// Writes a <see cref="string"/> value to the current position in the buffer and increases the position.
        /// </summary>
        /// <param name="value"> The <see cref="string"/> to write. </param>
        public void Write(string value)
        {
            Write((short)value.Length);
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            EnsureBufferSize(position + (bytes.Length << 3));
            BitWriter.WriteBytes(bytes, 0, bytes.Length, data, length);
            length += bytes.Length << 3;
        }

        /// <summary>
        /// Writes multiple <see cref="bool"/> to the buffer at the current position and increases the position.
        /// </summary>
        /// <param name="value"> The specific multiple <see cref="bool"/> to write. </param>
        public void Write(BitFlags value)
        {
            for (int i = 0; i < value.Capacity; i++)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Writes pad bits to reallign the buffer to the nearest byte.
        /// </summary>
        public void WritePadBits()
        {
            EnsureBufferSize(length = ((length + 7) >> 3) << 3);
        }

        /// <summary>
        /// Writes a specified amount of pad bits.
        /// </summary>
        /// <param name="amount"> The amount of bits to pad by. </param>
        public void WritePadBits(int amount)
        {
            EnsureBufferSize(length += amount);
        }
    }
}