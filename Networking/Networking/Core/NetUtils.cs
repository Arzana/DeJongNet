namespace DeJong.Networking.Core
{
    using Utilities.Core;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static partial class NetUtils
    {
        private static IPAddress broadcastAddress;
        private static IPAddress hostAddress;

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
        /// Attempt to get the host IP address from cache.
        /// </summary>
        /// <returns> The cahced address or a newly generated one. </returns>
        public static IPAddress GetHostAddress()
        {
            if (hostAddress == null) hostAddress = GetNewHostAddress();
            return hostAddress;
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

        public static IPAddress ResolveAddress(string host)
        {
            LoggedException.RaiseIf(string.IsNullOrEmpty(host), nameof(ResolveAddress), "Host cannot be empty");
            host.Trim();

            IPAddress result = null;
            if (IPAddress.TryParse(host, out result))
            {
                if (result.AddressFamily == AddressFamily.InterNetwork) return result;
                LoggedException.Raise("Only IPv4 addresses can be resolved");
            }

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses == null) return null;

                for (int i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily == AddressFamily.InterNetwork) return addresses[i];
                }

                return null;
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.HostNotFound)
                {
                    Log.Info(nameof(ResolveAddress), $"Host {host} cannot be found");
                    return null;
                }
                else
                {
                    LoggedException.Raise(nameof(ResolveAddress), "An error occured whilst resolving the remote host address", se);
                    throw;
                }
            }
        }

        private static byte[] ComputeSHAHash(byte[] bytes)
        {
            return ComputeSHAHash(bytes, 0, bytes.Length);
        }
    }
}