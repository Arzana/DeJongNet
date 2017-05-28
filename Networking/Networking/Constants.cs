namespace DeJong.Networking
{
    using Core.Messages;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class Constants
    {
        public const int MTU_ETHERNET_WITH_HEADERS = MTU_ETHERNET - IP_HEADER_MAX_SIZE - LibHeader.SIZE_BYTES;
        public static readonly uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
        public const int RTT_BUFFER_SIZE = 100;

        public const int MTU_MIN = LibHeader.SIZE_BYTES + FragmentHeader.SIZE_BYTES;
        public const int MTU_MAX = MTU_ETHERNET_WITH_HEADERS;

        private const int IP_HEADER_MAX_SIZE = 60;
        private const int MTU_ETHERNET = 1500;
        private const uint IOC_IN = 0x80000000;
        private const uint IOC_VENDOR = 0x18000000;
    }
}