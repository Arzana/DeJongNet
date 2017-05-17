namespace DeJong.Networking.Core.Messages
{
    internal enum MsgType : byte
    {
        LibraryError = 0,
        Unconnected = 1,
        Unreliable = 2,
        Ordered = 3,
        Reliable = 4,
        ReliableOrdered = 5,
        Ping = 6,
        Pong = 7,
        Connect = 8,     
        ConnectResponse = 9,
        ConnectionEstablished = 10,
        Acknowledge = 11,
        Disconnect = 12,
        Discovery = 13,
        DiscoveryResponse = 14,
        // TODO: Add NAT intro and MTU expand
    }
}