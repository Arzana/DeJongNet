namespace DeJong.Networking.Core.Messages
{
    using DataHandlers;
    using System.Text;

    public partial class ReadableBuffer : MsgBuffer
    {
        internal ReadableBuffer()
            : base()
        { }

        internal ReadableBuffer(byte[] buffer)
            : base(buffer)
        { }

        /// <summary>
        /// Reads the next bit from the buffer as a bool and increases the position by 1.
        /// </summary>
        /// <returns> The next bit as a bool. </returns> 
        public bool ReadBool()
        {
            bool result = PeekBool();
            PositionBits += 1;
            return result;
        }

        /// <summary>
        /// Reads the next 8 bits from the buffer as a byte and increases the position by 8.
        /// </summary>
        /// <returns> The next 8 bits as a byte. </returns>
        public byte ReadByte()
        {
            byte result = PeekByte();
            PositionBits += 8;
            return result;
        }

        /// <summary>
        /// Reads the next 8 bits from the buffer as a sbyte and increases the position by 8.
        /// </summary>
        /// <returns> The next 8 bits as a sbyte. </returns>
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        /// Reads the next 16 bits from the buffer as a short and increases the position by 16.
        /// </summary>
        /// <returns> The next 16 bits as a short. </returns>
        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        /// <summary>
        /// Reads the next 16 bits from the buffer as a ushort and increases the position by 16.
        /// </summary>
        /// <returns> The next 16 bits as a ushort. </returns>
        public ushort ReadUInt16()
        {
            ushort result = PeekUInt16();
            PositionBits += 16;
            return result;
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a int and increases the position by 32.
        /// </summary>
        /// <returns> The next 32 bits as a int. </returns>
        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a uint and increases the position by 32.
        /// </summary>
        /// <returns> The next 32 bits as a uint. </returns>
        public uint ReadUInt32()
        {
            uint result = PeekUInt32();
            PositionBits += 32;
            return result;
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a long and increases the position by 64.
        /// </summary>
        /// <returns> The next 64 bits as a long. </returns>
        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a ulong and increases the position by 64.
        /// </summary>
        /// <returns> The next 64 bits as a ulong. </returns>
        public ulong ReadUInt64()
        {
            ulong result = PeekUInt64();
            PositionBits += 64;
            return result;
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a float and increases the position by 32.
        /// </summary>
        /// <returns> The next 32 bits as a float. </returns>
        public float ReadSingle()
        {
            float result = PeekSingle();
            PositionBits += 32;
            return result;
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a double and increases the position by 64.
        /// </summary>
        /// <returns> The next 64 bits as a double. </returns>
        public double ReadDouble()
        {
            double result = PeekDouble();
            PositionBits += 64;
            return result;
        }

        /// <summary>
        /// Reads a variable amount of bits from the buffer as a string and increases the position.
        /// </summary>
        /// <returns> The next bits as a string. </returns>
        public string ReadString()
        {
            int length = ReadInt16();
            if (length < 1) return string.Empty;

            string result;
            if (BitAlligned) result = Encoding.UTF8.GetString(data, PositionBytes, length);
            else
            {
                byte[] bytes = new byte[length];
                BitReader.ReadBytes(data, PositionBits, length, bytes, 0);
                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            PositionBits += length << 3;
            return result;
        }

        /// <summary>
        /// Reads a specified amount of bits from the buffer as flags and increases the position.
        /// </summary>
        /// <param name="amount"> The specifified amount of bools to read. </param>
        /// <returns> The specified next bits a flags. </returns>
        public NetFlags ReadFlags(int amount)
        {
            NetFlags result = new NetFlags(amount);

            for (int i = 0; i < amount; i++)
            {
                result[i] = ReadBool();
            }

            return result;
        }

        /// <summary>
        /// Reads a variable amount of bits from the buffer as a byte and increases the position.
        /// </summary>
        /// <returns> The padding bits as a byte. </returns>
        public byte ReadPadBits()
        {
            byte result = PeekPadBits();
            SkipPadBits();
            return result;
        }

        /// <summary>
        /// Reads a specified amount of bits from the buffer as a byte and increases the position.
        /// </summary>
        /// <param name="amount"> The amount of bits to read. </param>
        /// <returns> The padding bits as a byte. </returns>
        public byte ReadPadBits(int amount)
        {
            byte result = PeekPadBits(amount);
            SkipPadBits(amount);
            return result;
        }

        /// <summary>
        /// Skips a variable amount of bits untill the position is byte alligned.
        /// </summary>
        public void SkipPadBits()
        {
            PositionBits = ((PositionBits + 7) >> 3) << 3;
        }

        /// <summary>
        /// Skips a specified amount of bits.
        /// </summary>
        /// <param name="amount"> The specified amount to skip. </param>
        public void SkipPadBits(int amount)
        {
            PositionBits += amount;
        }
    }
}