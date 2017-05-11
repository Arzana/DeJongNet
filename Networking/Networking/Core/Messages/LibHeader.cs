namespace DeJong.Networking.Core.Messages
{
    using Utilities;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines the packet header used for this library.
    /// </summary>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("[{ToString()}]")]
    internal struct LibHeader : IEquatable<LibHeader>
    {
        public const int SIZE_BYTES = 5;
        public const int SIZE_BITS = SIZE_BYTES << 3;

        public readonly MsgType Type;
        public readonly bool Fragment;
        public readonly int SequenceNumber;
        public readonly int PacketSize;

        public LibHeader(MsgBuffer buffer)
        {
            Type = (MsgType)buffer.ReadByte();
            Fragment = buffer.ReadBool();
            SequenceNumber = (buffer.ReadPadBits(7) << 8) | buffer.ReadByte();
            PacketSize = buffer.ReadInt16();
        }

        public LibHeader(MsgType type, bool isFragment, int sequenceNum, int dataSize)
        {
            Type = type;
            Fragment = isFragment;
            SequenceNumber = sequenceNum;
            PacketSize = dataSize;
        }

        public void WriteToBuffer(MsgBuffer buffer)
        {
            buffer.EnsureBufferSize(buffer.LengthBits + SIZE_BITS);
            buffer.Write((byte)Type);
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