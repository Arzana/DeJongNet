namespace DeJong.Networking.Core.Peers
{
    using System.Net;

    public sealed partial class Connection
    {
        public ConnectionStatus Status { get; private set; }
        public NetID RemoteID { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
    }
}