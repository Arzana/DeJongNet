namespace DeJong.Networking.Core.Messages
{
    using DataHandlers;
    using System.Text;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public class WriteableBuffer : MsgBuffer
    {
        internal WriteableBuffer() { }

        internal WriteableBuffer(byte[] buffer)
            : base(buffer)
        { }

        /// <summary>
        /// Writes a bool to the buffer as 1 bit and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(bool value)
        {
            EnsureBufferSize(LengthBits + 1);
            BitWriter.WriteByte(data, (byte)(value ? 1 : 0), LengthBits, 1);
            LengthBits += 1;
        }

        /// <summary>
        /// Writes a byte to the buffer as 8 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(byte value)
        {
            EnsureBufferSize(LengthBits + 8);
            BitWriter.WriteByte(data, value, LengthBits, 8);
            LengthBits += 8;
        }

        /// <summary>
        /// Writes a sbyte to the buffer as 8 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        /// <summary>
        /// Writes a short to the buffer as 16 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(short value)
        {
            Write((ushort)value);
        }

        /// <summary>
        /// Writes a ushort to the buffer as 16 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(ushort value)
        {
            EnsureBufferSize(LengthBits + 16);
            BitWriter.WriteUInt16(data, value, LengthBits, 16);
            LengthBits += 16;
        }

        /// <summary>
        /// Writes a int to the buffer as 32 bits and increases the lenght if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(int value)
        {
            Write((uint)value);
        }

        /// <summary>
        /// Writes a uint to the buffer as 32 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(uint value)
        {
            EnsureBufferSize(LengthBits + 32);
            BitWriter.WriteUInt32(data, value, LengthBits, 32);
            LengthBits += 32;
        }


        /// <summary>
        /// Writes a long to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(long value)
        {
            Write((ulong)value);
        }

        /// <summary>
        /// Writes a ulong to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(ulong value)
        {
            EnsureBufferSize(LengthBits + 64);
            BitWriter.WriteUInt64(data, value, LengthBits, 64);
            LengthBits += 64;
        }

        /// <summary>
        /// Writes a float to the buffer as 32 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(float value)
        {
            Write(new IntSingleUnion(value).IntValue);
        }

        /// <summary>
        /// Writes a double to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(double value)
        {
            Write(new IntDoubleUnion(value).IntValue);
        }

        /// <summary>
        /// Writes a string to the buffer and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value)) Write((short)0);
            else
            {
                Write((short)value.Length);
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                EnsureBufferSize(LengthBits + (bytes.Length << 3));
                BitWriter.WriteBytes(data, bytes, 0, bytes.Length, LengthBits);
                LengthBits += bytes.Length << 3;
            }
        }

        /// <summary>
        /// Writes flags to the buffer and increases the length if needed.
        /// </summary>
        /// <param name="value"> The value to write. </param>
        public void Write(NetFlags value)
        {
            for (int i = 0; i < value.Capacity; i++)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Writes padding bits to the buffer until the length is byte alligned.
        /// </summary>
        public void WritePadBits()
        {
            EnsureBufferSize(LengthBits = ((LengthBits + 7) >> 3) << 3);
        }

        /// <summary>
        /// Writes a specified amount of padding bits to the buffer.
        /// </summary>
        /// <param name="amount"> The amount of padding bits to write. </param>
        public void WritePadBits(int amount)
        {
            EnsureBufferSize(LengthBits += amount);
        }

        /// <summary>
        /// Writes a specified number of bits from a ulong to the buffer.
        /// </summary>
        /// <param name="value"> The container for the value. </param>
        /// <param name="bitAmount"> The amount of bits to write. </param>
        public void WritePartial(ulong value, int bitAmount)
        {
            EnsureBufferSize(LengthBits + bitAmount);
            BitWriter.WriteUInt64(data, value, LengthBits, bitAmount);
            LengthBits += bitAmount;
        }
    }
}