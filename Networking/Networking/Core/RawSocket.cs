﻿namespace DeJong.Networking.Core
{
    using Utilities.Logging;
    using System.Net.Sockets;
    using System.Net;
    using System;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class RawSocket
    {
        public byte[] SendBuffer { get; set; }
        public byte[] ReceiveBuffer { get; set; }

        private Socket socket;
        private PeerConfig config;

        private int listenPort;
        private double lastBindCall;

        public event StrongEventHandler<RawSocket, PacketReceiveEventArgs> PacketReceived;

        public RawSocket(PeerConfig config)
        {
            SendBuffer = new byte[config.SendBufferSize];
            ReceiveBuffer = new byte[config.ReceiveBufferSize];

            lastBindCall = float.MinValue;
            this.config = config;
        }

        public void Bind(bool reBind)
        {
            double now = NetTime.Now;
            if (now - lastBindCall < 1.0)
            {
                Log.Warning(nameof(RawSocket), $"Suppressed socket rebind; last bound {now - lastBindCall} seconds ago");
                return;
            }
            lastBindCall = now;

            if (socket == null) socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (reBind) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            socket.ReceiveBufferSize = config.ReceiveBufferSize;
            socket.SendBufferSize = config.SendBufferSize;
            socket.Blocking = false;

            socket.Bind(new IPEndPoint(config.LocalAddress, reBind ? listenPort : config.Port));

            try
            {
                socket.IOControl((int)Constants.SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            }
            catch
            {
                // Doesn't matter if this fails.
                Log.Warning(nameof(RawSocket), $"{nameof(Constants.SIO_UDP_CONNRESET)} not supported on this platform");
            }

            IPEndPoint boundEp = socket.LocalEndPoint as IPEndPoint;
            Log.Info(nameof(RawSocket), $"Socket bound to {boundEp}: {socket.IsBound}");
            listenPort = boundEp.Port;
        }

        public void UnBind()
        {
            try
            {
                if (socket != null)
                {
                    try { socket.Shutdown(SocketShutdown.Receive); }    // Operation mismatch?
                    catch (Exception ex) { Log.Warning(nameof(RawSocket), $"Unable to shutdown socket: {ex}"); }

                    try { socket.Close(2); }
                    catch (Exception ex) { Log.Warning(nameof(RawSocket), $"Unable to close socket: {ex}"); }
                }
            }
            finally
            {
                socket = null;
                lastBindCall = float.MinValue;
                SendBuffer = null;
                ReceiveBuffer = null;
            }
        }

        public void SendPacket(int numBytes, IPEndPoint target, out bool connectionReset)
        {
            connectionReset = false;
            IPAddress ba = default(IPAddress);
            if (socket == null) return;

            try
            {
                ba = NetUtils.GetBroadcastAddress();
                if (target.Address == ba) SetBroadcast(true);

                int bytesSend = socket.SendTo(SendBuffer, 0, numBytes, SocketFlags.None, target);
                if (numBytes != bytesSend) Log.Warning(nameof(RawSocket), $"Failed tp send full {numBytes} bytes packet; only {bytesSend} bytes send in packet");
            }
            catch (SocketException sx)
            {
                switch (sx.SocketErrorCode)
                {
                    case SocketError.WouldBlock:    // Send buffer full?
                        Log.Warning(nameof(RawSocket), "Socket threw exception; would block");
                        break;
                    case SocketError.ConnectionReset:
                        connectionReset = true;
                        break;
                }

                Log.Error(nameof(RawSocket), $"Failed to send packet: {sx}");
            }
            catch (Exception ex)
            {
                Log.Error(nameof(RawSocket), $"Failed to send packet: {ex}");
            }
            finally
            {
                if (target.Address == ba) SetBroadcast(false);
            }
        }

        public void ReceivePacket(ref EndPoint from)
        {
            if (socket == null) return;
            if (!socket.Poll(1000, SelectMode.SelectRead)) return;

            double now = NetTime.Now;
            do
            {
                int bytesReceived = 0;
                if (!TryReceivePacket(ref from, out bytesReceived)) return;

                if (bytesReceived < Constants.HEADER_BYTE_SIZE) return;
                EventInvoker.Invoke(PacketReceived, this, new PacketReceiveEventArgs(bytesReceived));

            } while (socket.Available > 0);
        }

        private bool TryReceivePacket(ref EndPoint from, out int bytesReceived)
        {
            try
            {
                bytesReceived = socket.ReceiveFrom(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, ref from);
                return true;
            }
            catch(SocketException sx)
            {
                bytesReceived = -1;

                switch (sx.SocketErrorCode)
                {
                    case (SocketError.ConnectionReset):
                        Log.Warning(nameof(RawSocket), "ConnectionsReset");
                        return false;
                    case (SocketError.NotConnected):
                        Bind(true);
                        return false;
                    default:
                        Log.Warning(nameof(RawSocket), $"Socket esception: {sx}");
                        return false;
                }
            }
        }

        private void SetBroadcast(bool value)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value);
        }
    }
}