namespace Mentula.Networking.Core
{
    using System;
    using System.Text;
    using static BitUtils;

    public partial class MsgBuffer
    {
        public bool ReadBool()
        {
            RaiseOverflowExceptionIf(1);
            byte result = BitExporter.ReadByte(data, 1, position);
            position += 1;
            return result != 0;
        }

        public byte ReadByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitExporter.ReadByte(data, BITS_PER_BYTE, position);
            position += BITS_PER_BYTE;
            return result;
        }

        public sbyte ReadSByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitExporter.ReadByte(data, BITS_PER_BYTE, position);
            position += BITS_PER_BYTE;
            return (sbyte)result;
        }

        public short ReadShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitExporter.ReadUInt16(data, BITS_PER_INT16, position);
            position += BITS_PER_INT16;
            return (short)result;
        }

        public ushort ReadUShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitExporter.ReadUInt16(data, BITS_PER_INT16, position);
            position += BITS_PER_INT16;
            return result;
        }

        public int ReadInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitExporter.ReadUInt32(data, BITS_PER_INT32, position);
            position += BITS_PER_INT32;
            return (int)result;
        }

        public uint ReadUInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitExporter.ReadUInt32(data, BITS_PER_INT32, position);
            position += BITS_PER_INT32;
            return result;
        }

        public long ReadLong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitExporter.ReadUInt64(data, BITS_PER_INT64, position);
            position += BITS_PER_INT64;
            return (long)result;
        }

        public ulong ReadULong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitExporter.ReadUInt64(data, BITS_PER_INT64, position);
            position += BITS_PER_INT64;
            return result;
        }

        public float ReadFloat()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            float result;

            if (BitsAlligned) result = BitConverter.ToSingle(data, position >> 3);
            else
            {
                byte[] bytes = new byte[sizeof(float)];
                BitExporter.ReadBytes(data, sizeof(float), position, bytes, 0);
                result = BitConverter.ToSingle(bytes, 0);
            }

            position += BITS_PER_INT32;
            return result;
        }

        public double ReadDouble()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            double result;

            if (BitsAlligned) result = BitConverter.ToDouble(data, position >> 3);
            else
            {
                byte[] bytes = new byte[sizeof(double)];
                BitExporter.ReadBytes(data, sizeof(double), position, bytes, 0);
                result = BitConverter.ToDouble(bytes, 0);
            }

            position += BITS_PER_INT64;
            return result;
        }

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
                BitExporter.ReadBytes(data, length, position, bytes, 0);
                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            position += length << 3;
            return result;
        }

        public BitFlags ReadFlags(int amount)
        {
            BitFlags result = new BitFlags(amount);
            RaiseOverflowExceptionIf(result.data.Length);

            for (int i = 0; amount > 0; i++)
            {
                result.data[i] = BitExporter.ReadByte(data, amount > BITS_PER_BYTE ? BITS_PER_BYTE : amount, position);
                amount -= BITS_PER_BYTE;
            }

            return result;
        }

        public void ReadPadBits()
        {
            position = ((position + 7) >> 3) << 3;
        }

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