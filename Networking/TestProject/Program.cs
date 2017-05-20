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

            client.DiscoverLocal(25565);

            using (ConsoleLogger cl = new ConsoleLogger { AutoUpdate = true })
            {
                Thread.Sleep(100);
                do
                {
                    server.PollMessages();
                    client.Disconnect("Testing");
                } while (Console.ReadKey().Key != ConsoleKey.Escape);

                Thread.Sleep(100);
            }
        }

        public static void TestDiscovery(IPEndPoint remote, EventArgs e)
        {
            Log.Info(nameof(Program), "Discovery received");
            server.SendDiscoveryResponse(null, remote);
        }
    }
}