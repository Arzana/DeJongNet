namespace DeJong.Networking.Core.Messages
{
    internal struct FragmentHeader
    {
        public int Group { get; private set; }      // Wich group of fragments this belongs to.
        public int TotalBits { get; private set; }  // Total number of bits in this group.
        public int ChunkSize { get; private set; }  // Size (in  bytes) of every chunk but the last one.
        public int ChunkNum { get; private set; }   // With number chunk this is, starts at zero.

        public FragmentHeader(MsgBuffer buffer)
        {
            Group = GetValue(buffer);
            TotalBits = GetValue(buffer);
            ChunkSize = GetValue(buffer);
            ChunkNum = GetValue(buffer);
        }

        public FragmentHeader(int group, int totalSize, int chunkSize, int chunkNum)
        {
            Group = group;
            TotalBits = totalSize;
            ChunkSize = chunkSize;
            ChunkNum = chunkNum;
        }

        public void Reset()
        {
            Group = 0;
            TotalBits = 0;
            ChunkSize = 0;
            ChunkNum = 0;
        }

        // Loop untill a value is assigned.
        // Read byte from the buffer
        // Mask byte into result
        // Check if single byte flag is set, if so; finalize result
        // 
        // | 1000 0010 | 0010 0100 |
        // 1 bit second byte flag (set if more bytes are needed for the value)
        // 2-8 bits first (or only) byte of the value
        // 9 bit third byte flag (set if more bytes are needed for the value)
        // 10-16 bits second (if flag is set) byte of the value
        // etc
        private static int GetValue(MsgBuffer buffer)
        {
            int temp = 0, byteIndex = 0, result = -1;

            while (result == -1)
            {
                byte raw = buffer.ReadByte();
                temp |= (raw & 0x7F) << (byteIndex & 0x1F);
                byteIndex += 7;

                if ((raw & 0x80) == 0) result = temp;
            }

            return result;
        }
    }
}