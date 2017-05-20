namespace DeJong.Networking.Core.Messages
{
    using DataHandlers;
    using System.Text;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public partial class ReadableBuffer : MsgBuffer
    {
        /// <summary>
        /// Reads the next bit from the buffer as a bool without increasing the position.
        /// </summary>
        /// <returns> The next bit as a bool. </returns>
        public bool PeekBool()
        {
            CheckOverflow(1);
            byte result = BitReader.ReadByte(data, PositionBits, 1);
            return result != 0;
        }

        /// <summary>
        /// Reads the next 8 bits from the buffer as a byte without increasing the position.
        /// </summary>
        /// <returns> The next 8 bits as a byte. </returns>
        public byte PeekByte()
        {
            CheckOverflow(8);
            byte result = BitReader.ReadByte(data, PositionBits, 8);
            return result;
        }

        /// <summary>
        /// Reads the next 8 bits from the buffer as a sbyte without increasing the position.
        /// </summary>
        /// <returns> The next 8 bits as a sbyte. </returns>
        public sbyte PeekSByte()
        {
            return (sbyte)PeekByte();
        }

        /// <summary>
        /// Reads the next 16 bits from the buffer as a short without increasing the position.
        /// </summary>
        /// <returns> The next 16 bits as a short. </returns>
        public short PeekInt16()
        {
            return (short)PeekUInt16();
        }

        /// <summary>
        /// Reads the next 16 bits from the buffer as a ushort without increasing the position.
        /// </summary>
        /// <returns> The next 16 bits as a ushort. </returns>
        public ushort PeekUInt16()
        {
            CheckOverflow(16);
            ushort result = BitReader.ReadUInt16(data, PositionBits, 16);
            return result;
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a int without increasing the position.
        /// </summary>
        /// <returns> The next 32 bits as a int. </returns>
        public int PeekInt32()
        {
            return (int)PeekUInt32();
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a uint without increasing the position.
        /// </summary>
        /// <returns> The next 32 bits as a uint. </returns>
        public uint PeekUInt32()
        {
            CheckOverflow(32);
            uint result = BitReader.ReadUInt32(data, PositionBits, 32);
            return result;
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a long without increasing the position.
        /// </summary>
        /// <returns> The next 64 bits as a long. </returns>
        public long PeekInt64()
        {
            return (long)PeekUInt64();
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a ulong without increasing the position.
        /// </summary>
        /// <returns> The next 64 bits as a ulong. </returns>
        public ulong PeekUInt64()
        {
            CheckOverflow(64);
            ulong result = BitReader.ReadUInt64(data, PositionBits, 64);
            return result;
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a float without increasing the position.
        /// </summary>
        /// <returns> The next 32 bits as a float. </returns>
        public float PeekSingle()
        {
            return new IntSingleUnion(PeekUInt32()).SingleValue;
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a double without increasing the position.
        /// </summary>
        /// <returns> The next 64 bits as a double. </returns>
        public double PeekDouble()
        {
            return new IntDoubleUnion(PeekUInt64()).DoubleValue;
        }

        /// <summary>
        /// Reads a variable amount of bits from the buffer as a string without increasing the position.
        /// </summary>
        /// <returns> The next bits as a string. </returns>
        public string PeekString()
        {
            int length = PeekInt16();
            if (length < 1) return string.Empty;

            CheckOverflow(16 + (length << 3));
            string result;

            if (BitAlligned) result = Encoding.UTF8.GetString(data, 16 + PositionBytes, length);
            else
            {
                byte[] bytes = new byte[length];
                BitReader.ReadBytes(data, PositionBits, length, bytes, 0);
                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            return result;
        }

        /// <summary>
        /// Reads the specified amount of bits from the buffer as flags without increasing the position.
        /// </summary>
        /// <param name="amount"> The amount of bits to read. </param>
        /// <returns> The specified next bits as flags. </returns>
        public NetFlags PeekFlags(int amount)
        {
            NetFlags result = new NetFlags(amount);

            for (int i = 0; i < amount; i++)
            {
                result[i] = PeekBool();
                ++PositionBits;
            }

            PositionBits -= amount;
            return result;
        }

        /// <summary>
        /// Reads a variable amount of bits from the buffer as a byte without increasing the position.
        /// </summary>
        /// <returns> The padding bits as a byte. </returns>
        public byte PeekPadBits()
        {
            int length = PositionBits % 8;
            CheckOverflow(length);
            return BitReader.ReadByte(data, PositionBits, length);
        }

        /// <summary>
        /// Reads a specified amount of bits from the buffer as a byte without increasing the position.
        /// </summary>
        /// <param name="amount"> The amount of bits to read. </param>
        /// <returns> The padding bits as a byte. </returns>
        public byte PeekPadBits(int amount)
        {
            CheckOverflow(amount);
            return BitReader.ReadByte(data, PositionBits, amount);
        }
    }
}