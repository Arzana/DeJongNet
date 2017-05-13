namespace DeJong.Networking.Core
{
    using System;
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

        /// <summary>
        /// Get a new remote indentifier from a specified endpoint.
        /// </summary>
        /// <returns> A SHA hash that represents this endpoint. </returns>
        public static long GetID(IPEndPoint ep)
        {
            byte[] macBytes = GetMacAdderssBytes();
            byte[] epBytes = BitConverter.GetBytes(ep.GetHashCode());

            byte[] combined = new byte[macBytes.Length + epBytes.Length];
            Array.Copy(epBytes, 0, combined, 0, epBytes.Length);
            Array.Copy(macBytes, 0, combined, epBytes.Length, macBytes.Length);

            return BitConverter.ToInt64(ComputeSHAHash(combined), 0);
        }

        public static string ToHexString(long id)
        {
            byte[] data = BitConverter.GetBytes(id);
            char[] result = new char[data.Length << 1];

            for (int i = 0; i < data.Length; i++)
            {
                byte high = (byte)(data[i] >> 0x4);
                result[i << 1] = (char)(high > 9 ? high + 0x37 : high + 0x30);

                byte low = (byte)(data[i] & 0xF);
                result[(i << 1) + 1] = (char)(low > 9 ? low + 0x37 : low + 0x30);
            }

            return new string(result);
        }

        private static byte[] ComputeSHAHash(byte[] bytes)
        {
            return ComputeSHAHash(bytes, 0, bytes.Length);
        }
    }
}