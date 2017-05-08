namespace DeJong.Networking.Core
{
    using System.Net;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static partial class NetUtils
    {
        private static IPAddress broadcastAddress;

        /// <summary>
        /// Attempts to get the broadcast address from cache.
        /// </summary>
        /// <returns> The cached address or a newly generated one. </returns>
        public static IPAddress GetBroadcastAddress()
        {
            if (broadcastAddress == null) broadcastAddress = GetNewBroadcastAddress();
            return broadcastAddress;
        }
    }
}