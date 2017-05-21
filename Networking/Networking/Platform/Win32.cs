#if !__ANDROID__ && !__CONSTRAINED__ && !WINDOWS_RUNTIME && !UNITY_STANDALONE_LINUX
namespace DeJong.Networking.Core
{
    using Utilities.Core;
    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Security.Cryptography;

    internal static partial class NetUtils
    {
        private static readonly SHA256 sha = SHA256.Create();

        public static byte[] ComputeSHAHash(byte[] bytes, int offset, int count)
        {
            return sha.ComputeHash(bytes, offset, count);
        }

        public static byte[] GetMacAdderssBytes()
        {
            NetworkInterface ni = GetNetworkInterface();
            return ni != null ? ni.GetPhysicalAddress().GetAddressBytes() : null;
        }

        private static IPAddress GetNewHostAddress()
        {
            NetworkInterface ni = GetNetworkInterface();
            if (ni == null) return null;

            IPInterfaceProperties props = ni.GetIPProperties();
            for (int i = 0; i < props.UnicastAddresses.Count; i++)
            {
                UnicastIPAddressInformation unicastAddress = props.UnicastAddresses[i];

                if (unicastAddress?.Address?.AddressFamily == AddressFamily.InterNetwork)
                {
                    return unicastAddress.Address;
                }
            }

            return null;
        }

        private static IPAddress GetNewBroadcastAddress()
        {
            NetworkInterface ni = GetNetworkInterface();
            if (ni == null) return null;

            IPInterfaceProperties props = ni.GetIPProperties();
            for (int i = 0; i < props.UnicastAddresses.Count; i++)
            {
                UnicastIPAddressInformation unicastAddress = props.UnicastAddresses[i];

                if (unicastAddress?.Address?.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] ipAddress = unicastAddress.Address.GetAddressBytes();
                    byte[] subnetMask = unicastAddress.IPv4Mask.GetAddressBytes();

                    LoggedException.RaiseIf(ipAddress.Length != subnetMask.Length, nameof(NetUtils), $"Cannot match IP address and subnet mask");

                    byte[] broadcast = new byte[ipAddress.Length];
                    for (int j = 0; j < broadcast.Length; j++)
                    {
                        broadcast[j] = (byte)(ipAddress[j] | (~subnetMask[j]));
                    }

                    return new IPAddress(broadcast);
                }
            }

            return IPAddress.Broadcast;
        }

        private static NetworkInterface GetNetworkInterface()
        {
            IPGlobalProperties computerProps = IPGlobalProperties.GetIPGlobalProperties();
            if (computerProps == null) return null;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (interfaces == null || interfaces.Length < 1) return null;

            NetworkInterface best = null;
            for (int i = 0; i < interfaces.Length; i++)
            {
                NetworkInterface adapter = interfaces[i];

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || adapter.NetworkInterfaceType == NetworkInterfaceType.Unknown) continue;
                if (!adapter.Supports(NetworkInterfaceComponent.IPv4)) continue;
                if (best == null) best = adapter;
                if (adapter.OperationalStatus != OperationalStatus.Up) continue;

                IPInterfaceProperties props = adapter.GetIPProperties();
                for (int j = 0; j < props.UnicastAddresses.Count; j++)
                {
                    UnicastIPAddressInformation unicastAddress = props.UnicastAddresses[j];
                    if (unicastAddress?.Address?.AddressFamily == AddressFamily.InterNetwork) return adapter;
                }
            }

            return best;
        }
    }

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static partial class NetTime
    {
        /// <summary>
        /// Defines what now is on windows platform (in seconds).
        /// </summary>
        public static double Now { get { return (Stopwatch.GetTimestamp() - initializedAt) * invFreq; } }

        private static readonly long initializedAt = Stopwatch.GetTimestamp();
        private static readonly double invFreq = 1.0 / Stopwatch.Frequency;
    }
}
#endif