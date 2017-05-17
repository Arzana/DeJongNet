namespace DeJong.Networking
{
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class Constants
    {
        public const int HEADER_BYTE_SIZE = 5;
        public const int IP_HEADER_MAX_SIZE = 60;

        public const int MTU_ETHERNET = 1500;
        public const int MTU_ETHERNET_WITH_HEADERS = MTU_ETHERNET - IP_HEADER_MAX_SIZE - HEADER_BYTE_SIZE;

        public const uint IOC_IN = 0x80000000;
        public const uint IOC_VENDOR = 0x18000000;
        public static readonly uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

        public const int DEFAULT_BUFFER_SIZE = 131071;
        public const int DEFAULT_RESEND_DELAY = 2;
    }
}