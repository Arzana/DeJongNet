namespace DeJong.Networking.Core.Peers
{
    using System.Net;

    public partial class Connection
    {
        public NetID ID { get; private set; }
        public ConnectionStatus Status { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
    }
}