namespace Mentula.Networking.Core
{
    using System;
    using System.Text;
    using static BitUtils;

    public partial class MsgBuffer
    {
        public bool PeekBool()
        {
            RaiseOverflowExceptionIf(1);
            byte result = BitExporter.ReadByte(data, 1, position);
            return result != 0;
        }

        public byte PeekByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitExporter.ReadByte(data, BITS_PER_BYTE, position);
            return result;
        }

        public sbyte PeekSByte()
        {
            RaiseOverflowExceptionIf(BITS_PER_BYTE);
            byte result = BitExporter.ReadByte(data, BITS_PER_BYTE, position);
            return (sbyte)result;
        }

        public short PeekShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitExporter.ReadUInt16(data, BITS_PER_INT16, position);
            return (short)result;
        }

        public ushort PeekUShort()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT16);
            ushort result = BitExporter.ReadUInt16(data, BITS_PER_INT16, position);
            return result;
        }

        public int PeekInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitExporter.ReadUInt32(data, BITS_PER_INT32, position);
            return (int)result;
        }

        public uint PeekUInt()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT32);
            uint result = BitExporter.ReadUInt32(data, BITS_PER_INT32, position);
            return result;
        }

        public long PeekLong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitExporter.ReadUInt64(data, BITS_PER_INT64, position);
            return (long)result;
        }

        public ulong PeekULong()
        {
            RaiseOverflowExceptionIf(BITS_PER_INT64);
            ulong result = BitExporter.ReadUInt64(data, BITS_PER_INT64, position);
            return result;
        }

        public float PeekFloat()
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

            return result;
        }

        public double PeekDouble()
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

            return result;
        }

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
                BitExporter.ReadBytes(data, length, position, bytes, 0);
                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            position -= BITS_PER_INT16;
            return result;
        }

        public BitFlags PeekFlags(int amount)
        {
            BitFlags result = new BitFlags(amount);
            RaiseOverflowExceptionIf(result.data.Length);

            int startPos = position;
            for (int i = 0; amount > 0; i++)
            {
                int bitsNum = amount > BITS_PER_BYTE ? BITS_PER_BYTE : amount;
                result.data[i] = BitExporter.ReadByte(data, bitsNum, position);
                amount -= BITS_PER_BYTE;
                position += bitsNum;
            }

            position = startPos;
            return result;
        }
    }
}
