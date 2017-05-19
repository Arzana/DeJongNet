namespace TestProject
{
    using DeJong.Networking.Core;
    using DeJong.Networking.Core.Peers;
    using System;
    using System.Net;

    public static class Program
    {
        public static void Main(string[] args)
        {
            NetServer server = new NetServer(new PeerConfig("TEST") { Port = 25565 });
            NetClient client = new NetClient(new PeerConfig("TEST"));

            server.OnDiscovery += TestDiscovery;
            server.Init();
            client.Init();

            client.DiscoverRemote(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 25565));

            client.Heartbeat();
            server.Heartbeat();
        }

        public static void TestDiscovery(IPEndPoint remote, EventArgs e)
        {

        }
    }
}