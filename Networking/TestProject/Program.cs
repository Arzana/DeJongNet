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
            server.OnDiscovery += Discovered;
            client.OnDiscoveryResponse += DiscoverResponse;
            server.OnConnect += Connected;
            server.OnStatusChanged += StatusChanged;
            client.OnStatusChanged += StatusChanged;

            client.DiscoverLocal(25565);

            using (ConsoleLogger cl = new ConsoleLogger { AutoUpdate = true })
            {
                Thread.Sleep(100);
                do
                {
                    server.PollMessages();
                    client.PollMessages();
                } while (Console.ReadKey().Key != ConsoleKey.Escape);

                client.Disconnect("Testing");
                Thread.Sleep(100);
            }
        }

        public static void Discovered(IPEndPoint remote, EventArgs e)
        {
            Log.Info(nameof(Program), "Server received discovery");
            server.SendDiscoveryResponse(null, remote);
        }

        public static void DiscoverResponse(Connection conn, SimpleMessageEventArgs e)
        {
            Log.Info(nameof(Program), "Client attempting to connect");
            client.Connect(conn, null);
        }

        private static void Connected(Connection sender, SimpleMessageEventArgs e)
        {
            Log.Info(nameof(Program), "Server accepting connect");
            server.AcceptConnection(sender, null);
        }

        private static void StatusChanged(Connection sender, StatusChangedEventArgs e)
        {
            Log.Verbose(nameof(Program), $"{sender.RemoteID} is now {e.NewStatus}");
        }
    }
}