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
        public readonly MsgType Type;
        public readonly bool Fragment;
        public readonly int SequenceNumber;
        public readonly int PacketSize;

        public LibHeader(byte[] data)
        {
            Type = (MsgType)data[0];
            Fragment = (data[1] & 0x80) != 0;
            SequenceNumber = ((data[1] & 0x7F) << 8) | data[2];
            PacketSize = (data[3] << 8) | data[4];
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