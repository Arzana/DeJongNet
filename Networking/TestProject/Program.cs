﻿namespace UnitTests
{
    using DeJong.Networking.Core;
    using DeJong.Networking.Core.Messages;
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
            server = new NetServer(new PeerConfig("TEST") { Port = 25565 });
            client = new NetClient(new PeerConfig("TEST"));
            server.OnDiscovery += Discovered;
            client.OnDiscoveryResponse += DiscoverResponse;
            server.OnConnect += Connected;
            server.OnStatusChanged += ServerStatusChanged;
            client.OnStatusChanged += ClientStatusChanged;
            server.OnDataMessage += OnDataMessage;

            client.AddChannel(1, DeliveryMethod.Unreliable);
            server.AddChannel(1, DeliveryMethod.Unreliable);

            client.DiscoverLocal(25565);

            using (ConsoleLogger cl = new ConsoleLogger { AutoUpdate = true })
            {
                Thread.Sleep(100);
                do
                {
                    server.PollMessages();
                    client.PollMessages();
                } while (!client.IsConnected);

                do
                {
                    OutgoingMsg msg = client.CreateMessage(1);
                    msg.Write("A very large test sting to test the maximum transmision unit");
                    client.Send(msg);

                    client.PollMessages();
                    server.PollMessages();
                } while (Console.ReadKey(false).Key != ConsoleKey.Escape);

                client.Disconnect("Testing");
                Thread.Sleep(100);
            }
        }

        private static void Discovered(IPEndPoint remote, EventArgs e)
        {
            Log.Debug("Server received discovery");
            server.SendDiscoveryResponse(null, remote);
        }

        private static void DiscoverResponse(Connection conn, SimpleMessageEventArgs e)
        {
            Log.Debug("Client attempting to connect");
            client.Connect(conn, null);
        }

        private static void Connected(Connection sender, SimpleMessageEventArgs e)
        {
            Log.Debug("Server accepting connect");
            server.AcceptConnection(sender, null);
        }

        private static void ServerStatusChanged(Connection sender, StatusChangedEventArgs e)
        {
            Log.Verbose(nameof(Program), $"{sender.RemoteID} is now {e.NewStatus} to server");
        }

        private static void ClientStatusChanged(Connection sender, StatusChangedEventArgs e)
        {
            Log.Verbose(nameof(Program), $"Client now {e.NewStatus} to {sender.RemoteID}");
        }

        private static void OnDataMessage(Connection sender, DataMessageEventArgs e)
        {
            Log.Debug(e.Message.ReadString());
        }
    }
}