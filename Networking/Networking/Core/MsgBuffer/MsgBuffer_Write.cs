namespace Mentula.Networking.Core
{
    using System;
    using System.Text;
    using static BitUtils;

    public partial class MsgBuffer
    {
        public void Write(bool value)
        {
            EnsureBufferSize(position + 1);
            BitExporter.WriteByte((byte)(value ? 1 : 0), 1, data, position);
            position += 1;
        }

        public void Write(byte value)
        {
            EnsureBufferSize(position + BITS_PER_BYTE);
            BitExporter.WriteByte(value, BITS_PER_BYTE, data, position);
            position += BITS_PER_BYTE;
        }

        public void Write(sbyte value)
        {
            EnsureBufferSize(position + BITS_PER_BYTE);
            BitExporter.WriteByte((byte)value, BITS_PER_BYTE, data, position);
            position += BITS_PER_BYTE;
        }

        public void Write(short value)
        {
            EnsureBufferSize(position + BITS_PER_INT16);
            BitExporter.WriteUInt16((ushort)value, BITS_PER_INT16, data, position);
            position += BITS_PER_INT16;
        }

        public void Write(ushort value)
        {
            EnsureBufferSize(position + BITS_PER_INT16);
            BitExporter.WriteUInt16(value, BITS_PER_INT16, data, position);
            position += BITS_PER_INT16;
        }

        public void Write(int value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            BitExporter.WriteUInt32((uint)value, BITS_PER_INT32, data, position);
            position += BITS_PER_INT32;
        }

        public void Write(uint value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            BitExporter.WriteUInt32(value, BITS_PER_INT32, data, position);
            position += BITS_PER_INT32;
        }

        public void Write(long value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            BitExporter.WriteUInt64((ulong)value, BITS_PER_INT64, data, position);
            position += BITS_PER_INT64;
        }

        public void Write(ulong value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            BitExporter.WriteUInt64(value, BITS_PER_INT64, data, position);
            position += BITS_PER_INT64;
        }

        public void Write(float value)
        {
            EnsureBufferSize(position + BITS_PER_INT32);
            byte[] bytes = BitConverter.GetBytes(value);
            BitExporter.WriteBytes(bytes, 0, sizeof(float), data, position);
            position += BITS_PER_INT32;
        }

        public void Write(double value)
        {
            EnsureBufferSize(position + BITS_PER_INT64);
            byte[] bytes = BitConverter.GetBytes(value);
            BitExporter.WriteBytes(bytes, 0, sizeof(double), data, position);
            position += BITS_PER_INT64;
        }

        public void Write(string value)
        {
            Write((short)value.Length);
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            EnsureBufferSize(position + (bytes.Length << 3));
            BitExporter.WriteBytes(bytes, 0, bytes.Length, data, position);
            position += bytes.Length << 3;
        }

        public void Write(BitFlags value)
        {
            for (int i = 0; i < value.Capacity; i++)
            {
                Write(value[i]);
            }
        }

        public void WritePadBits()
        {
            EnsureBufferSize(position = ((position + 7) >> 3) << 3);
        }

        public void WritePadBits(int amount)
        {
            EnsureBufferSize(position += amount);
        }
    }
}