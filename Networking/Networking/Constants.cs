namespace DeJong.Networking
{
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class Constants
    {
        /// <summary>
        /// The maximum transmision unit over IPv4.
        /// </summary>
        public const int MTU_ETHERNET = 1500;

        public const uint IOC_IN = 0x80000000;
        public const uint IOC_VENDOR = 0x18000000;
        /// <summary>
        /// This flag is used for windows XP support.
        /// </summary>
        public static readonly uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

        /// <summary>
        /// The default size for the receiving and sending buffer.
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 131071;

        /// <summary>
        /// 8 bits  - MessageType
        /// 1 bit   - Fragment
        /// 15 bits - Sequence Num
        /// 16 bits - Payload length in bits
        /// </summary>
        public const int HEADER_BYTE_SIZE = 5;
    }
}