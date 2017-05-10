﻿namespace DeJong.Networking.Core.Messages
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

        public LibHeader(MsgBuffer buffer)
        {
            Type = (MsgType)buffer.ReadByte();
            Fragment = buffer.ReadBool();
            SequenceNumber = (buffer.ReadPadBits(7) << 8) | buffer.ReadByte();
            PacketSize = buffer.ReadInt16();
        }

        public LibHeader(MsgType type, int fragGroup, int sequenceNum, int dataSize)
        {
            Type = type;
            Fragment = fragGroup != 0;
            SequenceNumber = sequenceNum;
            PacketSize = dataSize;
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