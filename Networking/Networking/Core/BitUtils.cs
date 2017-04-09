namespace Mentula.Networking.Core
{
    public static class BitUtils
    {
        public const int BITS_PER_BYTE = sizeof(byte) << 3;
        public const int BITS_PER_INT16 = sizeof(short) << 3;
        public const int BITS_PER_INT32 = sizeof(int) << 3;
        public const int BITS_PER_INT64 = sizeof(long) << 3;
    }
}