namespace DeJong.Networking.Core.Messages
{
    using Utilities;
    using System;
    using System.Diagnostics;

#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("[{ToString()}]")]
    internal struct LibHeader : IEquatable<LibHeader>
    {
        public const int SIZE_BYTES = 5;
        public const int SIZE_BITS = SIZE_BYTES << 3;

        public static readonly LibHeader Empty = new LibHeader();

        public readonly MsgType Type;
        public readonly int Channel;
        public readonly bool Fragment;
        public readonly int SequenceNumber;
        public readonly int PacketSize;

        public LibHeader(ReadableBuffer buffer)
        {
            Type = (MsgType)buffer.ReadPadBits(4);
            Channel = buffer.ReadPadBits(4);
            Fragment = buffer.ReadBool();
            SequenceNumber = buffer.ReadPadBits(7) | (buffer.ReadByte() << 3);
            PacketSize = buffer.ReadInt16();
        }

        public LibHeader(MsgType type, int channel, bool isFragment, int sequenceNum, int dataSize)
        {
            Type = type;
            Channel = channel;
            Fragment = isFragment;
            SequenceNumber = sequenceNum;
            PacketSize = dataSize;
        }

        public void WriteToBuffer(WriteableBuffer buffer)
        {
            buffer.EnsureBufferSize(buffer.LengthBits + SIZE_BITS);
            buffer.WritePartial((byte)((byte)Type & 0xF), 4);
            buffer.WritePartial((byte)(Channel & 0xF), 4);
            buffer.Write(Fragment);
            buffer.WritePartial((ulong)SequenceNumber, 15);
            buffer.Write((ushort)PacketSize);
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(LibHeader) ? Equals((LibHeader)obj) : false;
        }

        public bool Equals(LibHeader other)
        {
            return other.Type == Type
                && other.Fragment == Fragment
                && other.SequenceNumber == SequenceNumber
                && other.PacketSize == PacketSize;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Utils.HASH_BASE;
                hash += Utils.ComputeHash(hash, Type);
                hash += Utils.ComputeHash(hash, SequenceNumber);
                hash += Utils.ComputeHash(hash, PacketSize);
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Type} over {SequenceNumber} with size {PacketSize}";
        }
    }
}