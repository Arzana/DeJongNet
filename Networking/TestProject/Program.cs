namespace TestProject
{
    using DeJong.Networking.Core;
    using DeJong.Networking.Core.Peers;
    using DeJong.Utilities.Logging;
    using System;
    using System.Net;
    using System.Threading;

    public static class Program
    {
        private static NetServer server;
        private static NetClient client;

        public static void Main(string[] args)
        {
            server = new NetServer(new PeerConfig("TEST") { Port = 25565, });
            client = new NetClient(new PeerConfig("TEST"));
            server.OnDiscovery += TestDiscovery;

            client.DiscoverRemote(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 25565));

            using (ConsoleLogger cl = new ConsoleLogger { AutoUpdate = true })
            {
                do
                {
                    server.PollMessages();
                    client.PollMessages();
                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
        }

        public static void TestDiscovery(IPEndPoint remote, EventArgs e)
        {
            server.SendDiscoveryResponse(null, remote);
        }
    }
}