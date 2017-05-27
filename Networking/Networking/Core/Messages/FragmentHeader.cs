namespace DeJong.Networking.Core.Messages
{
    using Utilities;
    using System;
    using System.Diagnostics;

#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("[{ToString()}]")]
    internal struct FragmentHeader : IEquatable<FragmentHeader>
    {
        public const int SIZE_BYTES = 8;

        public int Group { get; private set; }          // Wich group of fragments this belongs to.
        public int TotalBits { get; private set; }      // Total number of bits in this group.
        public int FragmentSize { get; private set; }   // Size (in  bytes) of every chunk but the last one (probably).
        public int FragmentNum { get; private set; }    // With number chunk this is (starts at zero).

        public static readonly FragmentHeader Empty = new FragmentHeader();

        public FragmentHeader(ReadableBuffer buffer)
        {
            Group = buffer.ReadInt16();
            TotalBits = buffer.ReadInt16();
            FragmentSize = buffer.ReadInt16();
            FragmentNum = buffer.ReadInt16();
        }

        public FragmentHeader(int group, int totalSize, int chunkSize, int chunkNum)
        {
            Group = group;
            TotalBits = totalSize;
            FragmentSize = chunkSize;
            FragmentNum = chunkNum;
        }

        // Get maximum chunk size including the library header(5 bytes) and fragmentation header(8 bytes).
        // Create temporary header.
        // Update result.
        // Keep reducing fragment size until it fits within MTU.
        // Update number of fragments.
        public static int GetChunkSize(int group, int totalBytes, int mtu)
        {
            int result = mtu - LibHeader.SIZE_BYTES - 4;
            FragmentHeader tempHeader = new FragmentHeader(group, totalBytes, result, totalBytes / result);
            result = mtu - LibHeader.SIZE_BYTES - SIZE_BYTES;

            do
            {
                --result;

                int numChunks = totalBytes / result;
                if (numChunks * result < totalBytes) ++numChunks;

                tempHeader.FragmentSize = result;
                tempHeader.FragmentNum = numChunks;
            } while (result + SIZE_BYTES + LibHeader.SIZE_BYTES + 1 >= mtu);

            return result;
        }

        public void WriteToBuffer(WriteableBuffer buffer)
        {
            buffer.EnsureBufferSize(buffer.LengthBits + (SIZE_BYTES << 3));
            buffer.Write((short)Group);
            buffer.Write((short)TotalBits);
            buffer.Write((short)FragmentSize);
            buffer.Write((short)FragmentNum);
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(FragmentHeader) ? Equals((FragmentHeader)obj) : false;
        }

        public bool Equals(FragmentHeader other)
        {
            return other.Group == Group && other.FragmentNum == FragmentNum;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Utils.HASH_BASE;
                hash += Utils.ComputeHash(hash, Group);
                hash += Utils.ComputeHash(hash, FragmentNum);
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Group}#{FragmentNum} ({FragmentSize}/{TotalBits >> 3} bytes)";
        }
    }
}