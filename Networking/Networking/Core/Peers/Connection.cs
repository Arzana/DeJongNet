namespace DeJong.Networking.Core.Peers
{
    using System.Net;

    public partial class Connection
    {
        public long ID { get; private set; }
        public string ReadableID { get { return NetUtils.ToHexString(ID); } }
        public ConnectionStatus Status { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
    }
}