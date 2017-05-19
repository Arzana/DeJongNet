namespace DeJong.Networking.Core.Peers
{
    using Messages;
    using System;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;

    public sealed class NetServer : Peer
    {
        public event StrongEventHandler<IPEndPoint, EventArgs> OnDiscovery;

        public NetServer(PeerConfig config)
            : base(config)
        { }

        public void SendDiscoveryResponse(OutgoingMsg secMsg, IPEndPoint remoteHost)
        {
            AddConnection(remoteHost, secMsg);
        }

        protected override void HandleDiscovery(IPEndPoint sender, IncommingMsg msg)
        {
            if (msg.LengthBits > LibHeader.SIZE_BITS) Log.Warning(nameof(Peer), $"Invalid discovery message received from remote host {sender}, message dropped");
            else EventInvoker.InvokeSafe(OnDiscovery, sender, EventArgs.Empty);
        }
    }
}